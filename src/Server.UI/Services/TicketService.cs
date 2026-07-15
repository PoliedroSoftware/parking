using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Server.UI.Services;

public enum TicketType
{
    Entry,
    Exit,
    Payment,
    Wash,
    Monthly
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
    private const string Charset = "utf-8";
    private const int PageWidthMm = 80;
    private const int ContentWidthMm = 72;
    private const int MarginMm = 4;
    private const int FontSize = 10;
    private const int HeaderSize = 13;
    private const int SmallSize = 7;
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

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine($"<html><head><meta charset='{Charset}'>");
        sb.AppendLine("<meta name='viewport' content='width=device-width,initial-scale=1'>");
        sb.AppendLine("<script src='https://cdn.jsdelivr.net/npm/jsbarcode@3.11.6/dist/JsBarcode.all.min.js'></script>");
        sb.AppendLine("<style>");
        sb.AppendLine("* { margin: 0; padding: 0; box-sizing: border-box; }");
        sb.AppendLine("@media print {");
        sb.AppendLine($"  @page {{ size: {PageWidthMm}mm 200mm; margin: 0; }}");
        sb.AppendLine("  body { -webkit-print-color-adjust: exact; print-color-adjust: exact; }");
        sb.AppendLine("}");
        sb.AppendLine($"body {{ width: {ContentWidthMm}mm; margin: {MarginMm}mm; font-family: 'Courier New', monospace; font-size: {FontSize}px; color: #000; }}");
        sb.AppendLine(".center { text-align: center; }");
        sb.AppendLine(".bold { font-weight: bold; }");
        sb.AppendLine($".line {{ border-top: 1px dashed #000; margin: 4px 0; height: 0; }}");
        sb.AppendLine($".double-line {{ border-top: 2px solid #000; border-bottom: 1px solid #000; margin: 5px 0; padding: 2px 0; height: 0; }}");
        sb.AppendLine($".header {{ font-size: {HeaderSize}px; font-weight: bold; text-align: center; letter-spacing: 2px; }}");
        sb.AppendLine($".big {{ font-size: 15px; font-weight: bold; }}");
        sb.AppendLine($".huge {{ font-size: {HugeSize}px; font-weight: bold; }}");
        sb.AppendLine($".small {{ font-size: {SmallSize}px; }}");
        sb.AppendLine($".barcode-container {{ text-align: center; margin: 6px 0; }}");
        sb.AppendLine($".barcode-container svg {{ max-width: {ContentWidthMm - 6}mm; height: auto; }}");
        sb.AppendLine(".row { display: flex; justify-content: space-between; }");
        sb.AppendLine(".col { flex: 1; }");
        sb.AppendLine("</style></head><body>");

        // HEADER
        sb.AppendLine("<div class='header' style='text-transform:uppercase;'>POLIEDRO SOFTWARE</div>");
        sb.AppendLine($"<div class='center small'>{data.CarparkName}</div>");
        sb.AppendLine("<div class='center small'>NIT: 900.123.456-7 &bull; Bogota D.C.</div>");
        sb.AppendLine("<div class='line'></div>");

        // TICKET TYPE
        sb.AppendLine($"<div class='center bold' style='font-size:12px; margin:3px 0;'>{ticketType}</div>");
        sb.AppendLine("<div class='line'></div>");

        // DATE & TICKET NO
        sb.AppendLine($"<div class='row'><span>{data.DateTime:dd/MM/yyyy HH:mm}</span><span>No: {ticketNo}</span></div>");
        if (!string.IsNullOrEmpty(data.ZoneName))
            sb.AppendLine($"<div>Zona: {data.ZoneName}</div>");

        // ENTRY/EXIT TIMES
        if (data.EntryTime.HasValue && (data.Type == TicketType.Exit || data.Type == TicketType.Payment))
        {
            sb.AppendLine($"<div class='row'><span>Entrada:</span><span>{data.EntryTime.Value:dd/MM/yyyy HH:mm}</span></div>");
            sb.AppendLine($"<div class='row'><span>Salida:</span><span>{data.DateTime:dd/MM/yyyy HH:mm}</span></div>");
        }

        sb.AppendLine("<div class='line'></div>");

        // PLATE - BIG
        sb.AppendLine($"<div class='center'><span style='font-size:8px;'>PLACA</span></div>");
        sb.AppendLine($"<div class='center' style='font-size:22px; font-weight:900; margin:3px 0; letter-spacing:3px;'>{data.LicensePlate}</div>");

        if (!string.IsNullOrEmpty(data.VehicleType))
            sb.AppendLine($"<div class='center small'>Vehiculo: {data.VehicleType}</div>");

        if (!string.IsNullOrEmpty(data.CustomerName))
            sb.AppendLine($"<div class='center'>Cliente: {data.CustomerName}</div>");

        if (!string.IsNullOrEmpty(data.MemberName))
            sb.AppendLine($"<div class='center'>Miembro: {data.MemberName}</div>");

        // ENTRY TICKET SPECIFICS
        if (data.Type == TicketType.Entry)
        {
            sb.AppendLine("<div class='line'></div>");
            sb.AppendLine("<div class='center small'>Conserve este ticket</div>");
            sb.AppendLine("<div class='center small'>Presentelo para su salida</div>");
        }

        // EXIT / PAYMENT
        if (data.Type == TicketType.Exit || data.Type == TicketType.Payment)
        {
            if (data.Duration.HasValue)
            {
                var d = data.Duration.Value;
                var tiempo = d.TotalHours >= 1
                    ? $"{(int)d.TotalHours}h {d.Minutes}m"
                    : $"{d.Minutes} min";
                sb.AppendLine($"<div class='row'><span>Tiempo total:</span><span class='bold'>{tiempo}</span></div>");
            }
            if (data.Amount.HasValue)
            {
                sb.AppendLine("<div class='double-line'></div>");
                sb.AppendLine($"<div class='center bold huge'>$ {data.Amount.Value:N0}</div>");
                if (!string.IsNullOrEmpty(data.PaymentMethod))
                    sb.AppendLine($"<div class='center' style='margin-bottom:3px;'>Pago: {data.PaymentMethod}</div>");
                sb.AppendLine("<div class='double-line'></div>");
            }
        }

        // WASH TICKET
        if (data.Type == TicketType.Wash)
        {
            sb.AppendLine("<div class='line'></div>");
            if (data.QueueNumber.HasValue)
                sb.AppendLine($"<div class='center bold'>COLA #{data.QueueNumber.Value}</div>");
            if (!string.IsNullOrEmpty(data.WashServiceType))
                sb.AppendLine($"<div class='center bold'>Servicio: {data.WashServiceType}</div>");
            if (!string.IsNullOrEmpty(data.Notes))
                sb.AppendLine($"<div class='center small'>Incluye: {data.Notes}</div>");
            sb.AppendLine("<div class='line'></div>");
            if (data.Amount.HasValue)
            {
                if (data.BasePrice.HasValue && data.BasePrice > 0)
                    sb.AppendLine($"<div class='row'><span>Lavado base:</span><span>$ {data.BasePrice.Value:N0}</span></div>");
                if (data.AdditionalsTotal.HasValue && data.AdditionalsTotal > 0)
                    sb.AppendLine($"<div class='row'><span>Adicionales:</span><span>$ {data.AdditionalsTotal.Value:N0}</span></div>");
                if (data.Surcharge.HasValue && data.Surcharge > 0)
                    sb.AppendLine($"<div class='row'><span>Recargo fin sem.:</span><span>$ {data.Surcharge.Value:N0}</span></div>");
                sb.AppendLine("<div class='line'></div>");
                sb.AppendLine($"<div class='center bold huge'>$ {data.Amount.Value:N0}</div>");
                if (!string.IsNullOrEmpty(data.PaymentMethod))
                    sb.AppendLine($"<div class='center'>Pago: {data.PaymentMethod}</div>");
                if (!string.IsNullOrEmpty(data.CustomerName))
                    sb.AppendLine($"<div class='center small'>Cliente: {data.CustomerName}</div>");
                else
                    sb.AppendLine("<div class='center small'>Cliente: Consumidor Final</div>");
                if (!string.IsNullOrEmpty(data.OperatorName))
                    sb.AppendLine($"<div class='center small'>Operario(s): {data.OperatorName}</div>");
            }
        }

        // MONTHLY TICKET
        if (data.Type == TicketType.Monthly)
        {
            sb.AppendLine("<div class='line'></div>");
            if (data.Amount.HasValue)
            {
                sb.AppendLine($"<div class='row'><span>Valor alquiler:</span><span class='bold'>$ {data.Amount.Value:N0}</span></div>");
                if (!string.IsNullOrEmpty(data.PaymentMethod))
                    sb.AppendLine($"<div>Metodo: {data.PaymentMethod}</div>");
            }
        }

        // NOTES
        if (!string.IsNullOrEmpty(data.Notes))
        {
            sb.AppendLine("<div class='line'></div>");
            sb.AppendLine($"<div class='small'>{data.Notes}</div>");
        }

        // BARCODE
        sb.AppendLine("<div class='line'></div>");
        sb.AppendLine($"<div class='barcode-container'><svg id='barcode'></svg></div>");
        sb.AppendLine($"<div class='center small'>{ticketNo}</div>");

        // FOOTER
        sb.AppendLine("<div class='line'></div>");
        sb.AppendLine($"<div class='center small'>Impreso: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</div>");
        if (!string.IsNullOrEmpty(data.OperatorName))
            sb.AppendLine($"<div class='center small'>Operador: {data.OperatorName}</div>");
        if (!string.IsNullOrEmpty(data.CustomerName))
            sb.AppendLine($"<div class='center small'>Cliente: {data.CustomerName}</div>");
        else if (data.Type == TicketType.Exit || data.Type == TicketType.Payment)
            sb.AppendLine("<div class='center small'>Cliente: Consumidor Final</div>");
        sb.AppendLine("<div class='line'></div>");
        sb.AppendLine("<div class='center bold small'>GRACIAS POR SU VISITA</div>");
        sb.AppendLine("<div class='center small'>POLIEDRO SOFTWARE &bull; Soluciones de Parqueo</div>");
        sb.AppendLine("<div class='center small'>Tel: +57 (601) 123 4567 &bull; Bogota, Colombia</div>");

        // SCRIPTS: render barcode then print, close after
        sb.AppendLine("<script>");
        sb.AppendLine("try {");
        sb.AppendLine("  JsBarcode('#barcode', '" + ticketNo + "', {");
        sb.AppendLine("    format: 'CODE128',");
        sb.AppendLine("    width: 1.5,");
        sb.AppendLine("    height: 40,");
        sb.AppendLine("    displayValue: false,");
        sb.AppendLine("    margin: 2,");
        sb.AppendLine("    background: '#ffffff',");
        sb.AppendLine("    lineColor: '#000000'");
        sb.AppendLine("  });");
        sb.AppendLine("} catch(e) { console.error('Barcode error:', e); }");
        sb.AppendLine("window.onload = function() { window.print(); };");
        sb.AppendLine("window.onafterprint = function() { window.close(); };");
        sb.AppendLine("</script>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }

    public string GenerateTicketNumber()
    {
        var now = DateTime.Now;
        return $"TK{now:yyMMddHHmmss}{Random.Shared.Next(100, 999)}";
    }
}
