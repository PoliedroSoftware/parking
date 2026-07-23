using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CleanArchitecture.Blazor.Server.UI.Services;

public class TicketPdfService
{
    static TicketPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateTicketPdf(TicketData data)
    {
        var ticketType = GetTicketTitle(data.Type);

        var ticketNo = data.TicketNumber ?? $"TK{DateTime.Now:yyMMddHHmmss}{Random.Shared.Next(100, 999)}";
        var ticketVehicles = GetTicketVehicles(data);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(80, 297, Unit.Millimetre);
                page.Margin(3, Unit.Millimetre);
                page.DefaultTextStyle(s => s.FontFamily("Courier New").FontSize(8));

                page.Content().Column(col =>
                {
                    col.Item().AlignCenter().Text("POLIEDRO SOFTWARE").Bold().FontSize(11);
                    col.Item().AlignCenter().Text(data.CarparkName).FontSize(7);
                    col.Item().AlignCenter().Text("NIT: 900.123.456-7 - Bogota D.C.").FontSize(6);
                    col.Item().PaddingVertical(2).BorderBottom(1).BorderColor("#000");
                    col.Item().AlignCenter().Text(ticketType).Bold().FontSize(9);
                    col.Item().PaddingVertical(2).BorderBottom(1).BorderColor("#000");
                    if (data.Type == TicketType.Wash && data.IsPaid.HasValue)
                    {
                        col.Item()
                            .PaddingVertical(2)
                            .AlignCenter()
                            .Text(data.IsPaid.Value ? "PAGADO" : "NO PAGADO")
                            .Bold()
                            .FontSize(12);
                    }

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text(FormatDateTime(data.DateTime)).FontSize(7);
                        r.RelativeItem().AlignRight().Text($"No: {ticketNo}").FontSize(7);
                    });
                    var direction = GetParkingDirection(data.Type);
                    if (!string.IsNullOrEmpty(direction))
                        col.Item().Row(r => { r.RelativeItem().Text("Movimiento:").FontSize(7); r.RelativeItem().AlignRight().Text(direction).FontSize(7).Bold(); });

                    if (!string.IsNullOrEmpty(data.ZoneName))
                        col.Item().Text($"Zona: {data.ZoneName}").FontSize(7);

                    // Entry/Exit times
                    if (data.EntryTime.HasValue && (data.Type == TicketType.Exit || data.Type == TicketType.Payment))
                    {
                        col.Item().Row(r => { r.RelativeItem().Text("Entrada:").FontSize(7); r.RelativeItem().AlignRight().Text(FormatDateTime(data.EntryTime.Value)).FontSize(7); });
                        col.Item().Row(r => { r.RelativeItem().Text("Salida:").FontSize(7); r.RelativeItem().AlignRight().Text(FormatDateTime(data.DateTime)).FontSize(7); });
                        if (data.Duration.HasValue)
                            col.Item().Row(r => { r.RelativeItem().Text("Tiempo total:").FontSize(7); r.RelativeItem().AlignRight().Text(FormatDuration(data.Duration.Value)).Bold(); });
                    }

                    col.Item().PaddingVertical(2).BorderBottom(1).BorderColor("#000");

                    // Plate
                    if (data.Type == TicketType.Monthly && ticketVehicles.Count > 1)
                    {
                        col.Item().AlignCenter().Text("PLACAS").FontSize(6);
                        foreach (var vehicle in ticketVehicles)
                            col.Item().AlignCenter().Text(FormatVehicleLine(vehicle)).Bold().FontSize(8);
                    }
                    else
                    {
                        col.Item().AlignCenter().Text("PLACA").FontSize(6);
                        col.Item().AlignCenter().Text(GetPrimaryLicensePlate(data, ticketVehicles)).Bold().FontSize(16);
                    }

                    if (!string.IsNullOrEmpty(data.VehicleType))
                        col.Item().AlignCenter().Text($"Vehiculo: {data.VehicleType}").FontSize(7);

                    if (!string.IsNullOrEmpty(data.CustomerName))
                        col.Item().AlignCenter().Text($"Cliente: {data.CustomerName}").FontSize(7);
                    else if (data.Type == TicketType.Exit || data.Type == TicketType.Payment || data.Type == TicketType.Wash)
                        col.Item().AlignCenter().Text("Cliente: Consumidor Final").FontSize(7);

                    if (!string.IsNullOrEmpty(data.MemberName))
                        col.Item().AlignCenter().Text($"Miembro: {data.MemberName}").FontSize(7);

                    // Entry ticket specifics
                    if (data.Type == TicketType.Entry)
                    {
                        col.Item().PaddingVertical(2).BorderBottom(1).BorderColor("#000");
                        col.Item().AlignCenter().Text("Conserve este ticket").FontSize(7);
                        col.Item().AlignCenter().Text("Presentelo para su salida").FontSize(7);
                    }

                    // Exit / Payment
                    if (data.Type == TicketType.Exit || data.Type == TicketType.Payment)
                    {
                        if (data.HourlyRate.HasValue && data.HourlyRate > 0)
                            col.Item().Row(r => { r.RelativeItem().Text("Valor hora:").FontSize(7); r.RelativeItem().AlignRight().Text($"$ {data.HourlyRate.Value:N0}").FontSize(7).Bold(); });
                        if (data.Amount.HasValue)
                        {
                            col.Item().PaddingVertical(2).BorderBottom(1).BorderTop(1).BorderColor("#000").AlignCenter().Text($"$ {data.Amount.Value:N0}").Bold().FontSize(20);
                            if (!string.IsNullOrEmpty(data.PaymentMethod))
                                col.Item().AlignCenter().Text($"Pago: {data.PaymentMethod}").FontSize(7);
                        }
                    }

                    // Wash ticket
                    if (data.Type == TicketType.Wash)
                    {
                        col.Item().PaddingVertical(2).BorderBottom(1).BorderColor("#000");
                        if (data.QueueNumber.HasValue)
                            col.Item().AlignCenter().Text($"COLA #{data.QueueNumber.Value}").Bold().FontSize(9);
                        if (!string.IsNullOrEmpty(data.WashServiceType))
                            col.Item().AlignCenter().Text($"Servicio: {data.WashServiceType}").Bold();
                        AddAdditionalServices(col, data.Notes);
                        col.Item().PaddingVertical(2).BorderBottom(1).BorderColor("#000");

                        if (data.Amount.HasValue)
                        {
                            if (data.BasePrice.HasValue && data.BasePrice > 0)
                                col.Item().Row(r => { r.RelativeItem().Text("Lavado base:").FontSize(7); r.RelativeItem().AlignRight().Text($"$ {data.BasePrice.Value:N0}").FontSize(7); });
                            if (data.AdditionalsTotal.HasValue && data.AdditionalsTotal > 0)
                                col.Item().Row(r => { r.RelativeItem().Text("Adicionales:").FontSize(7); r.RelativeItem().AlignRight().Text($"$ {data.AdditionalsTotal.Value:N0}").FontSize(7); });
                            if (data.Surcharge.HasValue && data.Surcharge > 0)
                                col.Item().Row(r => { r.RelativeItem().Text("Recargo fin sem.:").FontSize(7); r.RelativeItem().AlignRight().Text($"$ {data.Surcharge.Value:N0}").FontSize(7); });
                            col.Item().PaddingVertical(2).BorderBottom(1).BorderColor("#000").AlignCenter().Text($"$ {data.Amount.Value:N0}").Bold().FontSize(20);
                            if (!string.IsNullOrEmpty(data.PaymentMethod))
                                col.Item().AlignCenter().Text($"Metodo de pago: {data.PaymentMethod}").FontSize(7);
                            if (!string.IsNullOrEmpty(data.OperatorName))
                                col.Item().AlignCenter().Text($"{GetWasherLabel(data.OperatorName)}: {data.OperatorName}").FontSize(6);
                        }
                    }

                    // Monthly
                    if (data.Type == TicketType.Monthly)
                    {
                        col.Item().PaddingVertical(2).BorderBottom(1).BorderColor("#000");
                        if (!string.IsNullOrEmpty(data.BillingPeriod))
                            col.Item().Row(r => { r.RelativeItem().Text("Mes pagado:").FontSize(7); r.RelativeItem().AlignRight().Text(data.BillingPeriod).Bold(); });
                        if (data.Amount.HasValue)
                        {
                            col.Item().Row(r => { r.RelativeItem().Text("Valor alquiler:").FontSize(7); r.RelativeItem().AlignRight().Text($"$ {data.Amount.Value:N0}").Bold(); });
                            if (!string.IsNullOrEmpty(data.PaymentMethod))
                                col.Item().Text($"Metodo de pago: {data.PaymentMethod}").FontSize(7);
                        }
                        AddPendingWashes(col, data.PendingWashes);
                    }

                    // Notes
                    if (!string.IsNullOrEmpty(data.Notes) && data.Type != TicketType.Wash)
                    {
                        col.Item().PaddingVertical(2).BorderBottom(1).BorderColor("#000");
                        col.Item().Text(data.Notes).FontSize(6);
                    }

                    // Footer
                    col.Item().PaddingVertical(2).BorderBottom(1).BorderColor("#000");
                    col.Item().AlignCenter().Text($"Impreso: {FormatDateTime(DateTime.Now, includeSeconds: true)}").FontSize(6);
                    if (!string.IsNullOrEmpty(data.OperatorName) && data.Type != TicketType.Wash)
                        col.Item().AlignCenter().Text($"Operador: {data.OperatorName}").FontSize(6);
                    col.Item().PaddingVertical(2).BorderBottom(1).BorderColor("#000");
                    col.Item().AlignCenter().Text("GRACIAS POR SU VISITA").Bold().FontSize(7);
                    col.Item().AlignCenter().Text("POLIEDRO SOFTWARE").FontSize(7);
                    col.Item().AlignCenter().Text("Tel: +57 (601) 123 4567 | Bogota, Colombia").FontSize(6);
                });
            });
        }).GeneratePdf();
    }

    private static string GetWasherLabel(string? operatorName)
    {
        var count = operatorName?
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Length ?? 0;

        return count == 1 ? "Lavador" : "Lavadores";
    }

    private static string GetTicketTitle(TicketType type)
    {
        return type switch
        {
            TicketType.Entry => "PARQUEO",
            TicketType.Exit => "PARQUEO",
            TicketType.Payment => "COMPROBANTE DE PAGO",
            TicketType.Wash => "LAVADO",
            TicketType.Monthly => "COMPROBANTE MENSUALIDAD",
            _ => "TICKET"
        };
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalDays >= 1)
            return $"{duration.Days}d {duration.Hours}h {duration.Minutes}m";

        return duration.TotalHours >= 1
            ? $"{(int)duration.TotalHours}h {duration.Minutes}m"
            : $"{duration.Minutes} min";
    }

    private static string FormatDateTime(DateTime value, bool includeSeconds = false)
    {
        var format = includeSeconds ? "dd/MM/yyyy hh:mm:ss tt" : "dd/MM/yyyy hh:mm tt";
        return value.ToString(format, CultureInfo.InvariantCulture);
    }

    private static void AddAdditionalServices(ColumnDescriptor col, string? notes)
    {
        var items = SplitAdditionalServices(notes);
        if (items.Length == 0)
            return;

        col.Item().Text("Incluye:").Bold().FontSize(6);
        foreach (var item in items)
            col.Item().Text($"- {item}").FontSize(6);
    }

    private static IReadOnlyList<TicketVehicleData> GetTicketVehicles(TicketData data)
    {
        if (data.Vehicles.Count > 0)
            return data.Vehicles;

        return string.IsNullOrWhiteSpace(data.LicensePlate)
            ? Array.Empty<TicketVehicleData>()
            : new[] { new TicketVehicleData { LicensePlate = data.LicensePlate, VehicleType = data.VehicleType } };
    }

    private static string GetPrimaryLicensePlate(TicketData data, IReadOnlyList<TicketVehicleData> vehicles)
    {
        return vehicles.Count > 0 ? vehicles[0].LicensePlate : data.LicensePlate;
    }

    private static string FormatVehicleLine(TicketVehicleData vehicle)
    {
        return string.IsNullOrWhiteSpace(vehicle.VehicleType)
            ? vehicle.LicensePlate
            : $"{vehicle.LicensePlate} - {vehicle.VehicleType}";
    }

    private static string FormatPendingWashLine(TicketPendingWashData wash)
    {
        var service = string.IsNullOrWhiteSpace(wash.Service) ? "Lavado" : wash.Service;
        var status = string.IsNullOrWhiteSpace(wash.Status) ? "Pendiente" : wash.Status;
        return $"{wash.LicensePlate} - {service} - {status} - $ {wash.Amount:N0}";
    }

    private static void AddPendingWashes(
        ColumnDescriptor col,
        IReadOnlyCollection<TicketPendingWashData> pendingWashes)
    {
        if (pendingWashes.Count == 0)
            return;

        col.Item().PaddingVertical(2).BorderBottom(1).BorderColor("#000");
        col.Item().Text("Lavados pendientes:").Bold().FontSize(6);
        foreach (var wash in pendingWashes)
            col.Item().Text($"- {FormatPendingWashLine(wash)}").FontSize(6);
    }

    private static string[] SplitAdditionalServices(string? notes)
    {
        return (notes ?? string.Empty)
            .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();
    }

    private static string? GetParkingDirection(TicketType type)
    {
        return type switch
        {
            TicketType.Entry => "Entrada",
            TicketType.Exit => "Salida",
            TicketType.Payment => "Salida",
            _ => null
        };
    }
}
