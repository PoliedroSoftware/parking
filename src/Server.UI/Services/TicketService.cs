using System.Net;
using System.Text.Json;
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
    public string? PaymentMethod { get; set; }
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
}

public class TicketService
{
    private int _ticketCounter;

    public string GenerateTicketNumber() =>
        $"TK{DateTime.Now:yyMMdd}{Interlocked.Increment(ref _ticketCounter):D4}";

    public string GenerateTicketHtmlForWindow(TicketData data) => GenerateTicketHtml(data);

    public string GenerateTicketText(TicketData data)
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
        var ticketNo = data.TicketNumber ?? GenerateTicketNumber();

        var sb = new System.Text.StringBuilder();

        // BLACK HEADER
        sb.AppendLine("[HEADER]");
        sb.AppendLine($"[CENTER-SMALL]:{data.CarparkName}");
        sb.AppendLine("[CENTER-TINY]:NIT: 900.123.456-7 | Bogota D.C.");
        sb.AppendLine("[DASHED]");

        // TICKET TYPE
        sb.AppendLine($"[CENTER]:{ticketType}");
        sb.AppendLine("[DASHED]");

        // DATE & TICKET NO
        sb.AppendLine($"[ROW]:{data.DateTime:dd/MM/yyyy HH:mm}|No: {ticketNo}");

        // ENTRY/EXIT TIMES in AM/PM
        if (data.EntryTime.HasValue && data.Type == TicketType.Exit)
        {
            var entrada = data.EntryTime.Value.ToString("hh:mm tt");
            var salida = data.DateTime.ToString("hh:mm tt");
            sb.AppendLine($"[ROW]:Entrada: {entrada}|Salida: {salida}");
        }
        if (!string.IsNullOrEmpty(data.VehicleType))
            sb.AppendLine($"[ROW]:Tipo:|{data.VehicleType}");
        if (!string.IsNullOrEmpty(data.OperatorName))
            sb.AppendLine($"[ROW]:Operador:|{data.OperatorName}");

        sb.AppendLine("[DASHED]");

        // HUGE LICENSE PLATE
        sb.AppendLine("[CENTER-SMALL]:PLACA");
        sb.AppendLine($"[HUGE]:{data.LicensePlate}");

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
            if (data.Duration.HasValue)
            {
                var d = data.Duration.Value;
                var t = d.TotalHours >= 1 ? $"{(int)d.TotalHours}h {d.Minutes}m" : $"{d.Minutes} min";
                sb.AppendLine($"[ROW]:Tiempo total:|{t}");
            }
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
                    sb.AppendLine($"[CENTER]:Pago: {data.PaymentMethod}");
                var op = data.OperatorName ?? "Operador";
                sb.AppendLine($"[CENTER-SMALL]:Operario(s): {op}");
            }
        }

        // BARCODE LINE
        sb.AppendLine("[DASHED]");
        sb.AppendLine($"[CENTER-SMALL]:{ticketNo}");

        // FOOTER
        sb.AppendLine("[DASHED]");
        sb.AppendLine($"[CENTER-SMALL]:Impreso: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        sb.AppendLine("[DASHED]");
        sb.AppendLine("[CENTER-TINY]:GRACIAS POR SU VISITA");
        sb.AppendLine("[CENTER-TINY]:POLIEDRO SOFTWARE | Soluciones de Parqueo");
        sb.AppendLine("[CENTER-TINY]:Tel: +57 (601) 123 4567 | Bogota, Colombia");

        return sb.ToString();
    }

    private static string FormatDuration(TimeSpan d)
    {
        if (d.TotalHours >= 1) return $"{(int)d.TotalHours}h {d.Minutes}m";
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
        var ticketType = data.Type switch
        {
            TicketType.Entry => "TICKET DE ENTRADA",
            TicketType.Exit => "TICKET DE SALIDA",
            TicketType.Payment => "COMPROBANTE DE PAGO",
            TicketType.Wash => "TICKET DE LAVADO",
            TicketType.Monthly => "COMPROBANTE MENSUALIDAD",
            _ => "TICKET"
        };

        var ticketNo = data.TicketNumber ?? GenerateTicketNumber();
        var carparkName = string.IsNullOrWhiteSpace(data.CarparkName) ? "POLIEDRO PARKING" : data.CarparkName;
        var barcodeValue = JsonSerializer.Serialize(ticketNo);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine($"<html><head><meta charset='{Charset}'>");
        sb.AppendLine("<meta name='viewport' content='width=device-width,initial-scale=1'>");
        sb.AppendLine("<script src='https://cdn.jsdelivr.net/npm/jsbarcode@3.11.6/dist/JsBarcode.all.min.js'></script>");
        sb.AppendLine("<style>");
        sb.AppendLine("* { margin: 0; padding: 0; box-sizing: border-box; }");
        sb.AppendLine($"html {{ width: {PageWidthMm}mm; background: #fff; }}");
        sb.AppendLine("@media print {");
        sb.AppendLine($"  @page {{ size: {PageWidthMm}mm {PreviewHeightMm}mm; margin: 0; }}");
        sb.AppendLine($"  html, body {{ width: {PageWidthMm}mm; min-width: {PageWidthMm}mm; }}");
        sb.AppendLine("  body { -webkit-print-color-adjust: exact; print-color-adjust: exact; }");
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
            "color: #000;",
            "background: #fff;");
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
        AppendStyle(sb, ".small", $"font-size: {SmallSize}px;", "font-weight: 900;");
        AppendStyle(sb, ".barcode-container", "text-align: center;", "margin: 7px 0 4px;");
        AppendStyle(sb, ".barcode-container svg", "max-width: 66mm;", "height: auto;");
        AppendStyle(sb, ".footer", "text-align: center;", "font-size: 9px;", "font-weight: 900;");
        sb.AppendLine("</style></head><body><div class='ticket'>");

        sb.AppendLine("<div class='brand'>");
        sb.AppendLine($"<div class='brand-name'>{Html(carparkName)}</div>");
        sb.AppendLine("<div class='brand-sub'>Parqueadero autorizado</div>");
        sb.AppendLine("<div class='brand-sub'>NIT: 900.123.456-7 - Bogota D.C.</div>");
        sb.AppendLine("</div>");
        sb.AppendLine($"<div class='doc-title'>{Html(ticketType)}</div>");
        AppendRow(sb, "Fecha", data.DateTime.ToString("dd/MM/yyyy HH:mm"));
        AppendRow(sb, "Ticket", ticketNo);
        if (!string.IsNullOrWhiteSpace(data.ZoneName))
            AppendRow(sb, "Zona", data.ZoneName);
        if (!string.IsNullOrWhiteSpace(data.OperatorName))
            AppendRow(sb, "Operador", data.OperatorName);
        sb.AppendLine("<div class='line'></div>");
        if (data.EntryTime.HasValue && (data.Type == TicketType.Exit || data.Type == TicketType.Payment))
        {
            AppendRow(sb, "Entrada", data.EntryTime.Value.ToString("dd/MM/yyyy HH:mm"));
            AppendRow(sb, "Salida", data.DateTime.ToString("dd/MM/yyyy HH:mm"));
        }

        sb.AppendLine("<div class='plate-box'>");
        sb.AppendLine("<div class='plate-label'>Placa</div>");
        sb.AppendLine($"<div class='plate-number'>{Html(data.LicensePlate)}</div>");
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
            if (data.Duration.HasValue)
            {
                var d = data.Duration.Value;
                var tiempo = d.TotalHours >= 1 ? $"{(int)d.TotalHours}h {d.Minutes}m" : $"{d.Minutes} min";
                AppendRow(sb, "Tiempo total", tiempo);
            }
            if (data.Amount.HasValue)
            {
                if (data.HourlyRate.HasValue && data.HourlyRate > 0)
                    AppendRow(sb, "Valor hora", Money(data.HourlyRate.Value));
                sb.AppendLine("<div class='total-box'>");
                sb.AppendLine("<div class='total-label'>Total pagado</div>");
                sb.AppendLine($"<div class='total-amount'>{Money(data.Amount.Value)}</div>");
                if (!string.IsNullOrWhiteSpace(data.PaymentMethod))
                    sb.AppendLine($"<div class='small'>Metodo: {Html(data.PaymentMethod)}</div>");
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
            if (!string.IsNullOrWhiteSpace(data.Notes))
                AppendRow(sb, "Incluye", data.Notes);
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
                    sb.AppendLine($"<div class='small'>Metodo: {Html(data.PaymentMethod)}</div>");
                sb.AppendLine("</div>");
                if (!string.IsNullOrWhiteSpace(data.CustomerName))
                    AppendRow(sb, "Cliente", data.CustomerName);
                else
                    AppendRow(sb, "Cliente", "Consumidor Final");
                if (!string.IsNullOrWhiteSpace(data.OperatorName))
                    AppendRow(sb, "Operario", data.OperatorName);
            }
        }

        if (data.Type == TicketType.Monthly && data.Amount.HasValue)
        {
            sb.AppendLine("<div class='line'></div>");
            AppendRow(sb, "Valor alquiler", Money(data.Amount.Value));
            if (!string.IsNullOrWhiteSpace(data.PaymentMethod))
                AppendRow(sb, "Metodo", data.PaymentMethod);
        }

        if (!string.IsNullOrWhiteSpace(data.Notes) && data.Type != TicketType.Wash)
        {
            sb.AppendLine("<div class='line'></div>");
            sb.AppendLine($"<div class='small'>{Html(data.Notes)}</div>");
        }

        sb.AppendLine("<div class='line'></div>");
        sb.AppendLine("<div class='barcode-container'><svg id='barcode'></svg></div>");
        sb.AppendLine($"<div class='center small'>{ticketNo}</div>");
        sb.AppendLine("<div class='line'></div>");
        sb.AppendLine("<div class='footer'>");
        sb.AppendLine($"<div>Impreso: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</div>");
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
        sb.AppendLine("try {");
        sb.AppendLine($"  JsBarcode('#barcode', {barcodeValue}, {{");
        sb.AppendLine("    format: 'CODE128', width: 1.5, height: 40,");
        sb.AppendLine("    displayValue: false, margin: 2,");
        sb.AppendLine("    background: '#ffffff', lineColor: '#000000'");
        sb.AppendLine("  });");
        sb.AppendLine("} catch(e) { console.error('Barcode error:', e); }");
        sb.AppendLine("window.onload = function() { setTimeout(function() { window.print(); }, 150); };");
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
}
