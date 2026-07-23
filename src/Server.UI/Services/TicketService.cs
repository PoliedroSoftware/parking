using System.Net;
using System.Globalization;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Server.UI.Services;

public enum TicketType
{
    Entry, Exit, Payment, Wash, Monthly
}

public class TicketData
{
    public TicketType Type { get; set; } = TicketType.Entry;
    public string CarparkName { get; set; } = string.Empty;
    public string ZoneName { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public DateTime DateTime { get; set; } = DateTime.Now;
    public DateTime? EntryTime { get; set; }
    public string? TicketNumber { get; set; }
    public decimal? Amount { get; set; }
    public decimal? HourlyRate { get; set; }
    public bool? IsPaid { get; set; }
    public string? PaymentMethod { get; set; }
    public string? BillingPeriod { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? OperatorName { get; set; }
    public string? MemberName { get; set; }
    public string? WashServiceType { get; set; }
    public string? CustomerName { get; set; }
    public string? Notes { get; set; }
    public int? QueueNumber { get; set; }
    public decimal? BasePrice { get; set; }
    public decimal? AdditionalsTotal { get; set; }
    public decimal? Surcharge { get; set; }
    public DateTime? EstimatedDelivery { get; set; }
    public List<TicketVehicleData> Vehicles { get; set; } = new();
    public List<TicketPendingWashData> PendingWashes { get; set; } = new();
}

public class TicketVehicleData
{
    public string LicensePlate { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
}

public class TicketPendingWashData
{
    public string LicensePlate { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class TicketService
{
    private int _ticketCounter;

    public string GenerateTicketNumber() =>
        $"TK{DateTime.Now:yyMMdd}{Interlocked.Increment(ref _ticketCounter):D4}";

    public string GenerateTicketHtmlForWindow(TicketData data) => GenerateTicketHtml(data);

    public string GenerateTicketText(TicketData data)
    {
        var ticketType = GetTicketTitle(data.Type);
        var ticketNo = data.TicketNumber ?? GenerateTicketNumber();

        var sb = new System.Text.StringBuilder();

        sb.AppendLine("[HEADER]");
        sb.AppendLine($"[CENTER-SMALL]:{data.CarparkName}");
        sb.AppendLine("[CENTER-TINY]:NIT: 900.123.456-7 | Bogota D.C.");
        sb.AppendLine("[DASHED]");

        // TICKET TYPE
        sb.AppendLine($"[CENTER]:{ticketType}");
        sb.AppendLine("[DASHED]");

        // DATE & TICKET NO
        sb.AppendLine($"[ROW]:{FormatDateTime(data.DateTime)}|No: {ticketNo}");
        var direction = GetParkingDirection(data.Type);
        if (!string.IsNullOrEmpty(direction))
            sb.AppendLine($"[ROW]:Movimiento:|{direction}");
        if (data.Type == TicketType.Wash && data.IsPaid.HasValue)
        {
            sb.AppendLine($"[CENTER]:{GetPaymentStatusText(data)}");
        }

        // ENTRY/EXIT TIMES in AM/PM
        if (data.EntryTime.HasValue && data.Type == TicketType.Exit)
        {
            var entrada = FormatTime(data.EntryTime.Value);
            var salida = FormatTime(data.DateTime);
            sb.AppendLine($"[ROW]:Entrada: {entrada}|Salida: {salida}");
            if (data.Duration.HasValue)
                sb.AppendLine($"[ROW]:Tiempo total:|{FormatDuration(data.Duration.Value)}");
        }
        if (!string.IsNullOrEmpty(data.VehicleType))
            sb.AppendLine($"[ROW]:Tipo:|{data.VehicleType}");
        if (!string.IsNullOrEmpty(data.OperatorName) && data.Type != TicketType.Wash)
            sb.AppendLine($"[ROW]:Operador:|{data.OperatorName}");

        sb.AppendLine("[DASHED]");

        var ticketVehicles = GetTicketVehicles(data);
        if (data.Type == TicketType.Monthly && ticketVehicles.Count > 1)
        {
            sb.AppendLine("[CENTER-SMALL]:PLACAS");
            foreach (var vehicle in ticketVehicles)
                sb.AppendLine($"[CENTER]:{FormatVehicleLine(vehicle)}");
        }
        else
        {
            sb.AppendLine("[CENTER-SMALL]:PLACA");
            sb.AppendLine($"[HUGE]:{GetPrimaryLicensePlate(data, ticketVehicles)}");
        }

        if (!string.IsNullOrEmpty(data.VehicleType))
            sb.AppendLine($"[CENTER-SMALL]:Vehiculo: {data.VehicleType}");
        if (!string.IsNullOrEmpty(data.CustomerName))
            sb.AppendLine($"[CENTER-SMALL]:Cliente: {data.CustomerName}");

        // ENTRY
        if (data.Type == TicketType.Entry)
        {
            sb.AppendLine("[DASHED]");
            sb.AppendLine("[CENTER-SMALL]:Conserve este ticket");
            sb.AppendLine("[CENTER-SMALL]:Presentelo para su salida");
        }

        // EXIT
        if (data.Type == TicketType.Exit && data.Amount.HasValue)
        {
            if (data.HourlyRate.HasValue && data.HourlyRate > 0)
                sb.AppendLine($"[ROW]:Valor hora:|$ {data.HourlyRate.Value:N0}");
            sb.AppendLine("[DOUBLE]");
            sb.AppendLine($"[HUGE]:$ {data.Amount.Value:N0}");
            if (!string.IsNullOrEmpty(data.PaymentMethod))
                sb.AppendLine($"[CENTER]:Pago: {data.PaymentMethod}");
            sb.AppendLine("[DOUBLE]");
        }

        // WASH
        if (data.Type == TicketType.Wash)
        {
            sb.AppendLine("[DASHED]");
            if (data.QueueNumber.HasValue)
                sb.AppendLine($"[CENTER]:COLA #{data.QueueNumber.Value}");
            if (!string.IsNullOrEmpty(data.WashServiceType))
                sb.AppendLine($"[CENTER]:Servicio: {data.WashServiceType}");
            AppendAdditionalServicesText(sb, data.Notes);
            if (data.Amount.HasValue)
            {
                sb.AppendLine("[DASHED]");
                if (data.BasePrice.HasValue && data.BasePrice > 0)
                    sb.AppendLine($"[ROW]:Lavado base:|$ {data.BasePrice:N0}");
                if (data.AdditionalsTotal.HasValue && data.AdditionalsTotal > 0)
                    sb.AppendLine($"[ROW]:Adicionales:|$ {data.AdditionalsTotal:N0}");
                if (data.Surcharge.HasValue && data.Surcharge > 0)
                    sb.AppendLine($"[ROW]:Recargo fin sem.:|$ {data.Surcharge:N0}");
                sb.AppendLine("[DASHED]");
                sb.AppendLine($"[HUGE]:$ {data.Amount:N0}");
                if (!string.IsNullOrEmpty(data.PaymentMethod))
                    sb.AppendLine($"[CENTER]:Metodo de pago: {data.PaymentMethod}");
                if (!string.IsNullOrEmpty(data.OperatorName))
                    sb.AppendLine($"[CENTER-SMALL]:{GetWasherLabel(data.OperatorName)}: {data.OperatorName}");
            }
        }

        if (data.Type == TicketType.Monthly && data.Amount.HasValue)
        {
            sb.AppendLine("[DASHED]");
            if (!string.IsNullOrEmpty(data.MemberName))
                sb.AppendLine($"[ROW]:Miembro:|{data.MemberName}");
            if (!string.IsNullOrEmpty(data.BillingPeriod))
                sb.AppendLine($"[ROW]:Mes pagado:|{data.BillingPeriod}");
            sb.AppendLine($"[ROW]:Valor alquiler:|$ {data.Amount.Value:N0}");
            if (!string.IsNullOrEmpty(data.PaymentMethod))
                sb.AppendLine($"[ROW]:Metodo de pago:|{data.PaymentMethod}");
            AppendPendingWashesText(sb, data.PendingWashes);
        }

        // BARCODE LINE
        sb.AppendLine("[DASHED]");
        sb.AppendLine($"[CENTER-SMALL]:{ticketNo}");

        // FOOTER
        sb.AppendLine("[DASHED]");
        sb.AppendLine($"[CENTER-SMALL]:Impreso: {FormatDateTime(DateTime.Now, includeSeconds: true)}");
        sb.AppendLine("[DASHED]");
        sb.AppendLine("[CENTER-TINY]:GRACIAS POR SU VISITA");
        sb.AppendLine("[CENTER-TINY]:POLIEDRO SOFTWARE | Soluciones de Parqueo");
        sb.AppendLine("[CENTER-TINY]:Tel: +57 (601) 123 4567 | Bogota, Colombia");

        return sb.ToString();
    }

    private static string FormatDuration(TimeSpan d)
    {
        if (d.TotalDays >= 1)
            return $"{d.Days}d {d.Hours}h {d.Minutes}m";
        if (d.TotalHours >= 1)
            return $"{(int)d.TotalHours}h {d.Minutes}m";
        return $"{d.Minutes} min";
    }

    private const string Charset = "utf-8";
    private const int PageWidthMm = 80;
    private const int ContentWidthMm = 72;
    private const int MarginMm = 4;
    private const int PreviewHeightMm = 297;
    private const int FontSize = 10;
    private const int HeaderSize = 13;
    private const int SmallSize = 8;
    private const int HugeSize = 22;

    public string GenerateTicketHtml(TicketData data)
    {
        var ticketType = GetTicketTitle(data.Type);

        var ticketNo = data.TicketNumber ?? GenerateTicketNumber();
        var carparkName = string.IsNullOrWhiteSpace(data.CarparkName) ? "POLIEDRO PARKING" : data.CarparkName;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine($"<html><head><meta charset='{Charset}'>");
        sb.AppendLine("<meta name='viewport' content='width=device-width,initial-scale=1'>");
        sb.AppendLine("<style>");
        sb.AppendLine("* { margin: 0; padding: 0; box-sizing: border-box; }");
        sb.AppendLine($"html {{ width: {PageWidthMm}mm; }}");
        sb.AppendLine("@media print {");
        sb.AppendLine($"  @page {{ size: {PageWidthMm}mm {PreviewHeightMm}mm; margin: 0; }}");
        sb.AppendLine($"  html, body {{ width: {PageWidthMm}mm; min-width: {PageWidthMm}mm; }}");
        sb.AppendLine("  .no-print { display: none !important; }");
        sb.AppendLine("}");
        AppendStyle(sb, "body",
            $"width: {PageWidthMm}mm;",
            $"min-width: {PageWidthMm}mm;",
            "margin: 0;",
            $"padding: {MarginMm}mm;",
            "font-family: 'Courier New', monospace;",
            $"font-size: {FontSize}px;",
            "font-weight: 800;",
            "line-height: 1.28;",
            "color: #000;");
        AppendStyle(sb, ".ticket",
            $"width: {ContentWidthMm}mm;",
            $"max-width: {ContentWidthMm}mm;",
            "overflow: hidden;");
        AppendStyle(sb, ".center", "text-align: center;");
        AppendStyle(sb, ".bold", "font-weight: 800;");
        AppendStyle(sb, ".brand",
            "padding: 1px 3px 2px;",
            "text-align: center;");
        AppendStyle(sb, ".brand-name",
            $"font-size: {HeaderSize}px;",
            "font-weight: 900;",
            "text-transform: uppercase;");
        AppendStyle(sb, ".brand-sub",
            "margin-top: 1px;",
            "font-size: 9px;",
            "font-weight: 900;",
            "text-transform: uppercase;");
        AppendStyle(sb, ".doc-title",
            "margin: 6px 0 5px;",
            "padding: 2px 0;",
            "text-align: center;",
            "font-size: 13px;",
            "font-weight: 900;",
            "text-transform: uppercase;");
        AppendStyle(sb, ".line", "border-top: 1px dashed #000;", "margin: 5px 0;", "height: 0;");
        AppendStyle(sb, ".solid-line", "border-top: 2px solid #000;", "margin: 5px 0;", "height: 0;");
        AppendStyle(sb, ".row",
            "display: flex;",
            "justify-content: space-between;",
            "gap: 5px;",
            "align-items: baseline;");
        AppendStyle(sb, ".row + .row", "margin-top: 2px;");
        AppendStyle(sb, ".label",
            "font-size: 10px;",
            "font-weight: 800;",
            "text-transform: uppercase;");
        AppendStyle(sb, ".value",
            "font-size: 10px;",
            "text-align: right;",
            "font-weight: 900;",
            "overflow-wrap: anywhere;");
        AppendStyle(sb, ".plate-box",
            "margin: 6px 0;",
            "padding: 5px 3px;",
            "border: 2px solid #000;",
            "text-align: center;");
        AppendStyle(sb, ".plate-label",
            "font-size: 10px;",
            "font-weight: 900;",
            "text-transform: uppercase;");
        AppendStyle(sb, ".plate-number",
            "margin-top: 2px;",
            $"font-size: {HugeSize}px;",
            "line-height: 1;",
            "font-weight: 900;");
        AppendStyle(sb, ".plate-list", "margin-top: 3px;");
        AppendStyle(sb, ".plate-list-item", "font-size: 12px;", "font-weight: 900;", "line-height: 1.2;");
        AppendStyle(sb, ".details", "margin-top: 4px;");
        AppendStyle(sb, ".notice",
            "margin: 5px 0;",
            "padding: 4px 3px;",
            "border-top: 1px dashed #000;",
            "border-bottom: 1px dashed #000;",
            "text-align: center;",
            "font-size: 9px;",
            "font-weight: 900;");
        AppendStyle(sb, ".total-box",
            "margin: 6px 0;",
            "padding: 5px 3px;",
            "border-top: 3px double #000;",
            "border-bottom: 3px double #000;",
            "text-align: center;");
        AppendStyle(sb, ".total-label",
            "font-size: 10px;",
            "font-weight: 900;",
            "text-transform: uppercase;");
        AppendStyle(sb, ".total-amount",
            "font-size: 22px;",
            "line-height: 1.05;",
            "font-weight: 900;");
        AppendStyle(sb, ".payment-status",
            "margin: 5px 0;",
            "padding: 2px 0;",
            "text-align: center;",
            "font-size: 13px;",
            "font-weight: 900;");
        AppendStyle(sb, ".payment-status.paid", "color: #000;");
        AppendStyle(sb, ".payment-status.unpaid", "color: #000;");
        AppendStyle(sb, ".additional-list", "margin: 3px 0 5px;");
        AppendStyle(sb, ".additional-title", "font-size: 10px;", "font-weight: 900;", "text-transform: uppercase;");
        AppendStyle(sb, ".additional-item", "font-size: 9px;", "font-weight: 900;", "padding-left: 3px;");
        AppendStyle(sb, ".small", $"font-size: {SmallSize}px;", "font-weight: 900;");
        AppendStyle(sb, ".footer", "text-align: center;", "font-size: 9px;", "font-weight: 900;");
        sb.AppendLine("</style></head><body><div class='ticket'>");

        sb.AppendLine("<div class='brand'>");
        sb.AppendLine($"<div class='brand-name'>{Html(carparkName)}</div>");
        sb.AppendLine("<div class='brand-sub'>Parqueadero autorizado</div>");
        sb.AppendLine("<div class='brand-sub'>NIT: 900.123.456-7 - Bogota D.C.</div>");
        sb.AppendLine("</div>");
        sb.AppendLine($"<div class='doc-title'>{Html(ticketType)}</div>");
        AppendRow(sb, "Fecha", FormatDateTime(data.DateTime));
        AppendRow(sb, "Ticket", ticketNo);
        AppendRow(sb, "Movimiento", GetParkingDirection(data.Type));
        if (data.Type == TicketType.Wash && data.IsPaid.HasValue)
        {
            var paymentClass = data.IsPaid.Value ? "paid" : "unpaid";
            sb.AppendLine($"<div class='payment-status {paymentClass}'>{GetPaymentStatusText(data)}</div>");
        }
        if (!string.IsNullOrWhiteSpace(data.ZoneName))
            AppendRow(sb, "Zona", data.ZoneName);
        if (!string.IsNullOrWhiteSpace(data.OperatorName) && data.Type != TicketType.Wash)
            AppendRow(sb, "Operador", data.OperatorName);
        sb.AppendLine("<div class='line'></div>");
        if (data.EntryTime.HasValue && (data.Type == TicketType.Exit || data.Type == TicketType.Payment))
        {
            AppendRow(sb, "Entrada", FormatDateTime(data.EntryTime.Value));
            AppendRow(sb, "Salida", FormatDateTime(data.DateTime));
            if (data.Duration.HasValue)
                AppendRow(sb, "Tiempo total", FormatDuration(data.Duration.Value));
        }

        var ticketVehicles = GetTicketVehicles(data);
        sb.AppendLine("<div class='plate-box'>");
        if (data.Type == TicketType.Monthly && ticketVehicles.Count > 1)
        {
            sb.AppendLine("<div class='plate-label'>Placas</div>");
            sb.AppendLine("<div class='plate-list'>");
            foreach (var vehicle in ticketVehicles)
                sb.AppendLine($"<div class='plate-list-item'>{Html(FormatVehicleLine(vehicle))}</div>");
            sb.AppendLine("</div>");
        }
        else
        {
            sb.AppendLine("<div class='plate-label'>Placa</div>");
            sb.AppendLine($"<div class='plate-number'>{Html(GetPrimaryLicensePlate(data, ticketVehicles))}</div>");
        }
        sb.AppendLine("</div>");

        sb.AppendLine("<div class='details'>");
        if (!string.IsNullOrWhiteSpace(data.VehicleType))
            AppendRow(sb, "Vehiculo", data.VehicleType);
        if (!string.IsNullOrWhiteSpace(data.CustomerName))
            AppendRow(sb, "Cliente", data.CustomerName);
        if (!string.IsNullOrWhiteSpace(data.MemberName))
            AppendRow(sb, "Miembro", data.MemberName);
        sb.AppendLine("</div>");

        if (data.Type == TicketType.Entry)
        {
            sb.AppendLine("<div class='notice'>Conserve este ticket y presentelo al salir.</div>");
        }

        if (data.Type == TicketType.Exit || data.Type == TicketType.Payment)
        {
            if (data.Amount.HasValue)
            {
                if (data.HourlyRate.HasValue && data.HourlyRate > 0)
                    AppendRow(sb, "Valor hora", Money(data.HourlyRate.Value));
                sb.AppendLine("<div class='total-box'>");
                sb.AppendLine("<div class='total-label'>Total pagado</div>");
                sb.AppendLine($"<div class='total-amount'>{Money(data.Amount.Value)}</div>");
                if (!string.IsNullOrWhiteSpace(data.PaymentMethod))
                    sb.AppendLine($"<div class='small'>Metodo de pago: {Html(data.PaymentMethod)}</div>");
                sb.AppendLine("</div>");
            }
        }

        if (data.Type == TicketType.Wash)
        {
            sb.AppendLine("<div class='line'></div>");
            if (data.QueueNumber.HasValue)
                sb.AppendLine($"<div class='center bold'>TURNO #{data.QueueNumber.Value}</div>");
            if (!string.IsNullOrWhiteSpace(data.WashServiceType))
                AppendRow(sb, "Servicio", data.WashServiceType);
            AppendAdditionalServicesHtml(sb, data.Notes);
            if (data.Amount.HasValue)
            {
                if (data.BasePrice.HasValue && data.BasePrice > 0)
                    AppendRow(sb, "Lavado base", Money(data.BasePrice.Value));
                if (data.AdditionalsTotal.HasValue && data.AdditionalsTotal > 0)
                    AppendRow(sb, "Adicionales", Money(data.AdditionalsTotal.Value));
                if (data.Surcharge.HasValue && data.Surcharge > 0)
                    AppendRow(sb, "Recargo", Money(data.Surcharge.Value));
                sb.AppendLine("<div class='total-box'>");
                sb.AppendLine("<div class='total-label'>Total servicio</div>");
                sb.AppendLine($"<div class='total-amount'>{Money(data.Amount.Value)}</div>");
                if (!string.IsNullOrWhiteSpace(data.PaymentMethod))
                    sb.AppendLine($"<div class='small'>Metodo de pago: {Html(data.PaymentMethod)}</div>");
                sb.AppendLine("</div>");
                if (!string.IsNullOrWhiteSpace(data.CustomerName))
                    AppendRow(sb, "Cliente", data.CustomerName);
                else
                    AppendRow(sb, "Cliente", "Consumidor Final");
                if (!string.IsNullOrWhiteSpace(data.OperatorName))
                    AppendRow(sb, GetWasherLabel(data.OperatorName), data.OperatorName);
            }
        }

        if (data.Type == TicketType.Monthly && data.Amount.HasValue)
        {
            sb.AppendLine("<div class='line'></div>");
            if (!string.IsNullOrWhiteSpace(data.BillingPeriod))
                AppendRow(sb, "Mes pagado", data.BillingPeriod);
            AppendRow(sb, "Valor alquiler", Money(data.Amount.Value));
            if (!string.IsNullOrWhiteSpace(data.PaymentMethod))
                AppendRow(sb, "Metodo de pago", data.PaymentMethod);
            AppendPendingWashesHtml(sb, data.PendingWashes);
        }

        if (!string.IsNullOrWhiteSpace(data.Notes) && data.Type != TicketType.Wash)
        {
            sb.AppendLine("<div class='line'></div>");
            sb.AppendLine($"<div class='small'>{Html(data.Notes)}</div>");
        }

        sb.AppendLine("<div class='line'></div>");
        sb.AppendLine($"<div class='center small'>{ticketNo}</div>");
        sb.AppendLine("<div class='line'></div>");
        sb.AppendLine("<div class='footer'>");
        sb.AppendLine($"<div>Impreso: {FormatDateTime(DateTime.Now, includeSeconds: true)}</div>");
        if (!string.IsNullOrWhiteSpace(data.CustomerName))
            sb.AppendLine($"<div>Cliente: {Html(data.CustomerName)}</div>");
        else if (data.Type == TicketType.Exit || data.Type == TicketType.Payment)
            sb.AppendLine("<div>Cliente: Consumidor Final</div>");
        sb.AppendLine("<div class='solid-line'></div>");
        sb.AppendLine("<div class='bold'>GRACIAS POR SU VISITA</div>");
        sb.AppendLine("<div>Poliedro Software - Soluciones de Parqueo</div>");
        sb.AppendLine("<div>Tel: +57 (601) 123 4567 - Bogota, Colombia</div>");
        sb.AppendLine("</div>");
        sb.AppendLine("<script>");
        sb.AppendLine("window.onload = function() { setTimeout(function() { window.print(); }, 250); };");
        sb.AppendLine("window.onafterprint = function() { setTimeout(function() { window.close(); }, 150); };");
        sb.AppendLine("</script></div></body></html>");
        return sb.ToString();
    }

    private static void AppendStyle(System.Text.StringBuilder sb, string selector, params string[] declarations)
    {
        sb.Append(selector);
        sb.Append(" { ");
        foreach (var declaration in declarations)
        {
            sb.Append(declaration);
            sb.Append(' ');
        }

        sb.AppendLine("}");
    }

    private static void AppendRow(System.Text.StringBuilder sb, string label, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        sb.AppendLine("<div class='row'>");
        sb.AppendLine($"<span class='label'>{Html(label)}</span>");
        sb.AppendLine($"<span class='value'>{Html(value)}</span>");
        sb.AppendLine("</div>");
    }

    private static string Html(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private static string Money(decimal amount) => $"$ {amount:N0}";

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
        return $"{wash.LicensePlate} - {service} - {status} - {Money(wash.Amount)}";
    }

    private static void AppendPendingWashesText(
        System.Text.StringBuilder sb,
        IReadOnlyCollection<TicketPendingWashData> pendingWashes)
    {
        if (pendingWashes.Count == 0)
            return;

        sb.AppendLine("[DASHED]");
        sb.AppendLine("[CENTER-SMALL]:Lavados pendientes:");
        foreach (var wash in pendingWashes)
            sb.AppendLine($"[CENTER-SMALL]:- {FormatPendingWashLine(wash)}");
    }

    private static void AppendPendingWashesHtml(
        System.Text.StringBuilder sb,
        IReadOnlyCollection<TicketPendingWashData> pendingWashes)
    {
        if (pendingWashes.Count == 0)
            return;

        sb.AppendLine("<div class='line'></div>");
        sb.AppendLine("<div class='additional-list'>");
        sb.AppendLine("<div class='additional-title'>Lavados pendientes:</div>");
        foreach (var wash in pendingWashes)
            sb.AppendLine($"<div class='additional-item'>- {Html(FormatPendingWashLine(wash))}</div>");
        sb.AppendLine("</div>");
    }

    private static string FormatDateTime(DateTime value, bool includeSeconds = false)
    {
        var format = includeSeconds ? "dd/MM/yyyy hh:mm:ss tt" : "dd/MM/yyyy hh:mm tt";
        return value.ToString(format, CultureInfo.InvariantCulture);
    }

    private static string FormatTime(DateTime value)
    {
        return value.ToString("hh:mm tt", CultureInfo.InvariantCulture);
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

    private static string GetPaymentStatusText(TicketData data)
    {
        return data.IsPaid == true ? "PAGADO" : "NO PAGADO";
    }

    private static void AppendAdditionalServicesText(System.Text.StringBuilder sb, string? notes)
    {
        var items = SplitAdditionalServices(notes);
        if (items.Length == 0)
            return;

        sb.AppendLine("[CENTER-SMALL]:Incluye:");
        foreach (var item in items)
            sb.AppendLine($"[CENTER-SMALL]:- {item}");
    }

    private static void AppendAdditionalServicesHtml(System.Text.StringBuilder sb, string? notes)
    {
        var items = SplitAdditionalServices(notes);
        if (items.Length == 0)
            return;

        sb.AppendLine("<div class='additional-list'>");
        sb.AppendLine("<div class='additional-title'>Incluye:</div>");
        foreach (var item in items)
            sb.AppendLine($"<div class='additional-item'>- {Html(item)}</div>");
        sb.AppendLine("</div>");
    }

    private static string[] SplitAdditionalServices(string? notes)
    {
        return (notes ?? string.Empty)
            .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();
    }

    private static string GetWasherLabel(string? operatorName)
    {
        var count = operatorName?
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Length ?? 0;

        return count == 1 ? "Lavador" : "Lavadores";
    }
}
