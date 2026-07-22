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
        var ticketType = data.Type switch
        {
            TicketType.Entry => "TICKET DE ENTRADA",
            TicketType.Exit => "TICKET DE SALIDA",
            TicketType.Payment => "COMPROBANTE DE PAGO",
            TicketType.Wash => "TICKET DE LAVADO",
            TicketType.Monthly => "COMPROBANTE MENSUALIDAD",
            _ => "TICKET"
        };

        var ticketNo = data.TicketNumber ?? $"TK{DateTime.Now:yyMMddHHmmss}{Random.Shared.Next(100, 999)}";

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(80, 200, Unit.Millimetre);
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

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text(data.DateTime.ToString("dd/MM/yyyy HH:mm")).FontSize(7);
                        r.RelativeItem().AlignRight().Text($"No: {ticketNo}").FontSize(7);
                    });

                    if (!string.IsNullOrEmpty(data.ZoneName))
                        col.Item().Text($"Zona: {data.ZoneName}").FontSize(7);

                    // Entry/Exit times
                    if (data.EntryTime.HasValue && (data.Type == TicketType.Exit || data.Type == TicketType.Payment))
                    {
                        col.Item().Row(r => { r.RelativeItem().Text("Entrada:").FontSize(7); r.RelativeItem().AlignRight().Text(data.EntryTime.Value.ToString("dd/MM/yyyy HH:mm")).FontSize(7); });
                        col.Item().Row(r => { r.RelativeItem().Text("Salida:").FontSize(7); r.RelativeItem().AlignRight().Text(data.DateTime.ToString("dd/MM/yyyy HH:mm")).FontSize(7); });
                    }

                    col.Item().PaddingVertical(2).BorderBottom(1).BorderColor("#000");

                    // Plate
                    col.Item().AlignCenter().Text("PLACA").FontSize(6);
                    col.Item().AlignCenter().Text(data.LicensePlate).Bold().FontSize(16);

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
                        if (data.Duration.HasValue)
                        {
                            var d = data.Duration.Value;
                            var tiempo = d.TotalHours >= 1 ? $"{(int)d.TotalHours}h {d.Minutes}m" : $"{d.Minutes} min";
                            col.Item().Row(r => { r.RelativeItem().Text("Tiempo total:").FontSize(7); r.RelativeItem().AlignRight().Text(tiempo).Bold(); });
                        }
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
                        if (!string.IsNullOrEmpty(data.Notes))
                            col.Item().AlignCenter().Text($"Incluye: {data.Notes}").FontSize(6);
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
                                col.Item().AlignCenter().Text($"Pago: {data.PaymentMethod}").FontSize(7);
                            if (!string.IsNullOrEmpty(data.OperatorName))
                                col.Item().AlignCenter().Text($"Operario(s): {data.OperatorName}").FontSize(6);
                        }
                    }

                    // Monthly
                    if (data.Type == TicketType.Monthly)
                    {
                        col.Item().PaddingVertical(2).BorderBottom(1).BorderColor("#000");
                        if (data.Amount.HasValue)
                        {
                            col.Item().Row(r => { r.RelativeItem().Text("Valor alquiler:").FontSize(7); r.RelativeItem().AlignRight().Text($"$ {data.Amount.Value:N0}").Bold(); });
                            if (!string.IsNullOrEmpty(data.PaymentMethod))
                                col.Item().Text($"Metodo: {data.PaymentMethod}").FontSize(7);
                        }
                    }

                    // Notes
                    if (!string.IsNullOrEmpty(data.Notes) && data.Type != TicketType.Wash)
                    {
                        col.Item().PaddingVertical(2).BorderBottom(1).BorderColor("#000");
                        col.Item().Text(data.Notes).FontSize(6);
                    }

                    // Footer
                    col.Item().PaddingVertical(2).BorderBottom(1).BorderColor("#000");
                    col.Item().AlignCenter().Text($"Impreso: {DateTime.Now:dd/MM/yyyy HH:mm:ss}").FontSize(6);
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
}
