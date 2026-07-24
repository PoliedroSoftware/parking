using System.Globalization;
using System.Net;

namespace CleanArchitecture.Blazor.Server.UI.Services;

public sealed class CierreReportData
{
    public DateTime Date { get; set; }
    public int TotalEntries { get; set; }
    public int TotalExits { get; set; }
    public int TotalWashes { get; set; }
    public int TotalMonthly { get; set; }
    public int TotalParkingRevenue { get; set; }
    public int TotalWashRevenue { get; set; }
    public int TotalMonthlyRevenue { get; set; }
    public int TotalRevenue { get; set; }
    public List<CierreMovementData> Movements { get; set; } = new();
    public TicketCompanyData Company { get; set; } = TicketCompanyData.Default;
}

public sealed class CierreMovementData
{
    public DateTime Time { get; set; }
    public string Type { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string FormattedDuration { get; set; } = string.Empty;
    public int Amount { get; set; }
}

public sealed class UtilidadReportData
{
    public int Year { get; set; }
    public List<MonthlyProfitData> Months { get; set; } = new();
    public int TotalParkingRevenue { get; set; }
    public int TotalWashRevenue { get; set; }
    public int TotalMonthlyRevenue { get; set; }
    public int TotalExpenses { get; set; }
    public int TotalProfit { get; set; }
    public TicketCompanyData Company { get; set; } = TicketCompanyData.Default;
}

public sealed class MonthlyProfitData
{
    public string MonthName { get; set; } = string.Empty;
    public int ParkingRevenue { get; set; }
    public int WashRevenue { get; set; }
    public int MonthlyRevenue { get; set; }
    public int Expenses { get; set; }
    public int Profit { get; set; }
}

public sealed class IngresosReportData
{
    public DateTime Date { get; set; }
    public int ParkingRevenue { get; set; }
    public int WashRevenue { get; set; }
    public int MonthlyRevenue { get; set; }
    public int TotalExpenses { get; set; }
    public int NetRevenue { get; set; }
    public List<DayRevenueData> Days { get; set; } = new();
    public List<ExpenseItemData> Expenses { get; set; } = new();
    public TicketCompanyData Company { get; set; } = TicketCompanyData.Default;
}

public sealed class DayRevenueData
{
    public string Day { get; set; } = string.Empty;
    public int Parking { get; set; }
    public int Wash { get; set; }
    public int Monthly { get; set; }
    public int Total { get; set; }
}

public sealed class ExpenseItemData
{
    public string Date { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Amount { get; set; }
}

public sealed class ReportTicketService(CompanyInformationService companyInformationService)
{
    private const string Charset = "utf-8";
    private const int PageWidthMm = 80;
    private const int ContentWidthMm = 72;
    private const int MarginMm = 4;
    private const int PreviewHeightMm = 297;
    private const int FontSize = 10;
    private const int SmallSize = 8;
    private const int HugeSize = 22;

    public async Task<(string TaggedText, string Html)> GenerateCierreTicketAsync(
        CierreReportData data, CancellationToken cancellationToken = default)
    {
        data.Company = await companyInformationService.GetAsync(cancellationToken);
        return (GenerateCierreText(data), GenerateCierreHtml(data));
    }

    public async Task<(string TaggedText, string Html)> GenerateUtilidadTicketAsync(
        UtilidadReportData data, CancellationToken cancellationToken = default)
    {
        data.Company = await companyInformationService.GetAsync(cancellationToken);
        return (GenerateUtilidadText(data), GenerateUtilidadHtml(data));
    }

    public async Task<(string TaggedText, string Html)> GenerateIngresosTicketAsync(
        IngresosReportData data, CancellationToken cancellationToken = default)
    {
        data.Company = await companyInformationService.GetAsync(cancellationToken);
        return (GenerateIngresosText(data), GenerateIngresosHtml(data));
    }

    private static string GenerateCierreText(CierreReportData data)
    {
        var sb = new System.Text.StringBuilder();
        AppendReportHeader(sb, data.Company, "ARQUEO DE CAJA");

        sb.AppendLine($"[ROW]:Fecha:|{data.Date:dd/MM/yyyy}");
        sb.AppendLine("[DASHED]");
        sb.AppendLine($"[ROW]:Entradas:|{data.TotalEntries}");
        sb.AppendLine($"[ROW]:Salidas:|{data.TotalExits}");
        sb.AppendLine($"[ROW]:Lavados:|{data.TotalWashes}");
        sb.AppendLine($"[ROW]:Mensualidades:|{data.TotalMonthly}");
        sb.AppendLine("[DASHED]");
        sb.AppendLine($"[ROW-BOLD]:PARKING:|$ {data.TotalParkingRevenue:N0}");
        sb.AppendLine($"[ROW-BOLD]:LAVADO:|$ {data.TotalWashRevenue:N0}");
        sb.AppendLine($"[ROW-BOLD]:MENSUALIDAD:|$ {data.TotalMonthlyRevenue:N0}");
        sb.AppendLine("[DOUBLE]");
        sb.AppendLine($"[HUGE]:$ {data.TotalRevenue:N0}");
        sb.AppendLine("[DASHED]");

        if (data.Movements.Count > 0)
        {
            sb.AppendLine("[CENTER-SMALL]:MOVIMIENTOS DEL DIA");
            sb.AppendLine("[DASHED]");
            foreach (var m in data.Movements)
            {
                var cliente = string.IsNullOrWhiteSpace(m.CustomerName) ? "CF" : m.CustomerName;
                var label = m.Type switch
                {
                    "LAVADO" => "LA",
                    "MENSUAL" => "ME",
                    _ => "PQ"
                };
                sb.AppendLine($"[ROW]:{FormatTime(m.Time)} {label} {m.LicensePlate}|{(m.Amount > 0 ? $"$ {m.Amount:N0}" : "-")}");
            }
        }

        AppendReportFooter(sb, data.Company);
        return sb.ToString();
    }

    private static string GenerateCierreHtml(CierreReportData data)
    {
        var sb = StartHtml();
        AppendReportHtmlHeader(sb, data.Company, "ARQUEO DE CAJA");
        AppendHtmlRow(sb, "Fecha", data.Date.ToString("dd/MM/yyyy"));
        AppendHtmlLine(sb);
        AppendHtmlRow(sb, "Entradas", data.TotalEntries.ToString());
        AppendHtmlRow(sb, "Salidas", data.TotalExits.ToString());
        AppendHtmlRow(sb, "Lavados", data.TotalWashes.ToString());
        AppendHtmlRow(sb, "Mensualidades", data.TotalMonthly.ToString());
        AppendHtmlLine(sb);
        AppendHtmlRowBold(sb, "PARKING", Money(data.TotalParkingRevenue));
        AppendHtmlRowBold(sb, "LAVADO", Money(data.TotalWashRevenue));
        AppendHtmlRowBold(sb, "MENSUALIDAD", Money(data.TotalMonthlyRevenue));
        AppendHtmlDouble(sb);
        AppendHtmlTotal(sb, Money(data.TotalRevenue));
        AppendHtmlLine(sb);

        if (data.Movements.Count > 0)
        {
            sb.AppendLine("<div class='plate-label'>MOVIMIENTOS DEL DIA</div>");
            sb.AppendLine("<div class='line'></div>");
            foreach (var m in data.Movements)
            {
                var cliente = string.IsNullOrWhiteSpace(m.CustomerName) ? "CF" : m.CustomerName;
                var label = m.Type switch { "LAVADO" => "LA", "MENSUAL" => "ME", _ => "PQ" };
                AppendHtmlRow(sb, $"{FormatTime(m.Time)} {label} {m.LicensePlate}",
                    m.Amount > 0 ? Money(m.Amount) : "-");
            }
        }

        AppendReportHtmlFooter(sb, data.Company);
        return FinishHtml(sb);
    }

    private static string GenerateUtilidadText(UtilidadReportData data)
    {
        var sb = new System.Text.StringBuilder();
        AppendReportHeader(sb, data.Company, "REPORTE DE UTILIDAD");
        sb.AppendLine($"[CENTER]:Ano: {data.Year}");
        sb.AppendLine("[DASHED]");

        foreach (var m in data.Months)
        {
            sb.AppendLine($"[CENTER]:{m.MonthName.Substring(0, 3).ToUpperInvariant()}");
            sb.AppendLine($"[ROW]:Parqueo:|$ {m.ParkingRevenue:N0}");
            sb.AppendLine($"[ROW]:Lavado:|$ {m.WashRevenue:N0}");
            sb.AppendLine($"[ROW]:Mensualidad:|$ {m.MonthlyRevenue:N0}");
            sb.AppendLine($"[ROW]:Gastos:|$ {m.Expenses:N0}");
            sb.AppendLine($"[ROW-BOLD]:Utilidad:|$ {m.Profit:N0}");
            sb.AppendLine("[DASHED]");
        }

        sb.AppendLine("[DOUBLE]");
        sb.AppendLine($"[HUGE]:$ {data.TotalProfit:N0}");
        sb.AppendLine($"[CENTER]:UTILIDAD ANUAL");
        sb.AppendLine("[DASHED]");

        AppendReportFooter(sb, data.Company);
        return sb.ToString();
    }

    private static string GenerateUtilidadHtml(UtilidadReportData data)
    {
        var sb = StartHtml();
        AppendReportHtmlHeader(sb, data.Company, "REPORTE DE UTILIDAD");
        sb.AppendLine($"<div class='doc-title'>Ano: {data.Year}</div>");
        AppendHtmlLine(sb);

        foreach (var m in data.Months)
        {
            sb.AppendLine(
                $"<div class='plate-label'>{Html(m.MonthName[..Math.Min(3, m.MonthName.Length)].ToUpperInvariant())}</div>");
            AppendHtmlRow(sb, "Parqueo", Money(m.ParkingRevenue));
            AppendHtmlRow(sb, "Lavado", Money(m.WashRevenue));
            AppendHtmlRow(sb, "Mensualidad", Money(m.MonthlyRevenue));
            AppendHtmlRow(sb, "Gastos", Money(m.Expenses));
            AppendHtmlRowBold(sb, "Utilidad", Money(m.Profit));
            AppendHtmlLine(sb);
        }

        AppendHtmlDouble(sb);
        AppendHtmlTotal(sb, Money(data.TotalProfit));
        sb.AppendLine("<div class='total-label'>UTILIDAD ANUAL</div>");
        AppendHtmlLine(sb);
        AppendReportHtmlFooter(sb, data.Company);
        return FinishHtml(sb);
    }

    private static string GenerateIngresosText(IngresosReportData data)
    {
        var sb = new System.Text.StringBuilder();
        AppendReportHeader(sb, data.Company, "REPORTE MENSUAL");
        sb.AppendLine($"[CENTER]:{data.Date:MMMM yyyy}");
        sb.AppendLine("[DASHED]");
        sb.AppendLine($"[ROW]:Parqueo:|$ {data.ParkingRevenue:N0}");
        sb.AppendLine($"[ROW]:Lavado:|$ {data.WashRevenue:N0}");
        sb.AppendLine($"[ROW]:Mensualidad:|$ {data.MonthlyRevenue:N0}");
        sb.AppendLine($"[ROW]:Gastos:|$ {data.TotalExpenses:N0}");
        sb.AppendLine("[DOUBLE]");
        sb.AppendLine($"[HUGE]:$ {data.NetRevenue:N0}");
        sb.AppendLine($"[CENTER]:RESULTADO NETO");
        sb.AppendLine("[DASHED]");

        if (data.Days.Count > 0)
        {
            sb.AppendLine("[DIVIDER]");
            sb.AppendLine("[CENTER]:DETALLE DIARIO");
            sb.AppendLine("[DASHED]");
            foreach (var d in data.Days)
            {
                sb.AppendLine($"[CENTER]:{d.Day}");
                if (d.Parking > 0)
                    sb.AppendLine($"[ROW]:Parqueo|$ {d.Parking:N0}");
                if (d.Wash > 0)
                    sb.AppendLine($"[ROW]:Lavado|$ {d.Wash:N0}");
                if (d.Monthly > 0)
                    sb.AppendLine($"[ROW]:Mensualidad|$ {d.Monthly:N0}");
                sb.AppendLine($"[ROW-BOLD]:TOTAL DIA|$ {d.Total:N0}");
                sb.AppendLine("[DASHED]");
            }

            sb.AppendLine("[DOUBLE]");
            sb.AppendLine($"[ROW-BOLD]:TOTAL Parqueo|$ {data.Days.Sum(d => d.Parking):N0}");
            sb.AppendLine($"[ROW-BOLD]:TOTAL Lavado|$ {data.Days.Sum(d => d.Wash):N0}");
            sb.AppendLine($"[ROW-BOLD]:TOTAL Mensualidad|$ {data.Days.Sum(d => d.Monthly):N0}");
            sb.AppendLine($"[HUGE]:$ {data.Days.Sum(d => d.Total):N0}");
            sb.AppendLine("[DOUBLE]");
        }

        if (data.Expenses.Count > 0)
        {
            sb.AppendLine("[DIVIDER]");
            sb.AppendLine("[CENTER-SMALL]:GASTOS");
            sb.AppendLine("[DASHED]");
            foreach (var e in data.Expenses.Take(20))
            {
                sb.AppendLine($"[ROW]:{e.Date} {e.Description}|$ {e.Amount:N0}");
            }

            sb.AppendLine("[DASHED]");
            sb.AppendLine($"[ROW-BOLD]:Total gastos:|$ {data.TotalExpenses:N0}");
        }

        AppendReportFooter(sb, data.Company);
        return sb.ToString();
    }

    private static string GenerateIngresosHtml(IngresosReportData data)
    {
        var sb = StartHtml();
        AppendReportHtmlHeader(sb, data.Company, "REPORTE MENSUAL");
        sb.AppendLine($"<div class='doc-title'>{data.Date:MMMM yyyy}</div>");
        AppendHtmlLine(sb);
        AppendHtmlRow(sb, "Parqueo", Money(data.ParkingRevenue));
        AppendHtmlRow(sb, "Lavado", Money(data.WashRevenue));
        AppendHtmlRow(sb, "Mensualidad", Money(data.MonthlyRevenue));
        AppendHtmlRow(sb, "Gastos", Money(data.TotalExpenses));
        AppendHtmlDouble(sb);
        AppendHtmlTotal(sb, Money(data.NetRevenue));
        sb.AppendLine("<div class='total-label'>RESULTADO NETO</div>");
        AppendHtmlLine(sb);

        if (data.Days.Count > 0)
        {
            sb.AppendLine("<div class='line' style='border-top:2px solid #000;'></div>");
            sb.AppendLine("<div class='plate-label'>DETALLE DIARIO</div>");
            sb.AppendLine("<div class='line'></div>");

            sb.AppendLine("<table style='width:100%;border-collapse:collapse;'>");
            sb.AppendLine("<tr style='font-size:8px;font-weight:900;text-transform:uppercase;'>");
            sb.AppendLine("<td style='padding:2px 4px;'>Dia</td>");
            sb.AppendLine("<td style='padding:2px 4px;text-align:right;'>Parqueo</td>");
            sb.AppendLine("<td style='padding:2px 4px;text-align:right;'>Lavado</td>");
            sb.AppendLine("<td style='padding:2px 4px;text-align:right;'>Mens.</td>");
            sb.AppendLine("<td style='padding:2px 4px;text-align:right;'>Total</td>");
            sb.AppendLine("</tr>");

            foreach (var d in data.Days)
            {
                sb.AppendLine("<tr style='border-top:1px dotted #ccc;'>");
                sb.AppendLine($"<td style='padding:2px 4px;font-weight:900;'>{Html(d.Day)}</td>");
                sb.AppendLine($"<td style='padding:2px 4px;text-align:right;'>{Money(d.Parking)}</td>");
                sb.AppendLine($"<td style='padding:2px 4px;text-align:right;'>{Money(d.Wash)}</td>");
                sb.AppendLine($"<td style='padding:2px 4px;text-align:right;'>{Money(d.Monthly)}</td>");
                sb.AppendLine($"<td style='padding:2px 4px;text-align:right;font-weight:900;'>{Money(d.Total)}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("<tr style='border-top:2px solid #000;font-weight:900;'>");
            sb.AppendLine("<td style='padding:2px 4px;'>TOTAL</td>");
            sb.AppendLine($"<td style='padding:2px 4px;text-align:right;'>{Money(data.Days.Sum(d => d.Parking))}</td>");
            sb.AppendLine($"<td style='padding:2px 4px;text-align:right;'>{Money(data.Days.Sum(d => d.Wash))}</td>");
            sb.AppendLine($"<td style='padding:2px 4px;text-align:right;'>{Money(data.Days.Sum(d => d.Monthly))}</td>");
            sb.AppendLine($"<td style='padding:2px 4px;text-align:right;'>{Money(data.Days.Sum(d => d.Total))}</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</table>");

            AppendHtmlLine(sb);
        }

        if (data.Expenses.Count > 0)
        {
            AppendHtmlSolid(sb);
            sb.AppendLine("<div class='plate-label'>GASTOS</div>");
            AppendHtmlLine(sb);
            foreach (var e in data.Expenses.Take(20))
            {
                AppendHtmlRow(sb, $"{e.Date} {e.Description}", Money(e.Amount));
            }

            AppendHtmlLine(sb);
            AppendHtmlRowBold(sb, "Total gastos", Money(data.TotalExpenses));
        }

        AppendReportHtmlFooter(sb, data.Company);
        return FinishHtml(sb);
    }

    private static void AppendReportHeader(System.Text.StringBuilder sb, TicketCompanyData company,
        string title)
    {
        sb.AppendLine("[HEADER]");
        sb.AppendLine($"[CENTER-SMALL]:{company.TradeName}");
        sb.AppendLine($"[CENTER-TINY]:{company.TaxId} | {company.Address}");
        sb.AppendLine("[DASHED]");
        sb.AppendLine($"[CENTER]:{title}");
        sb.AppendLine("[DASHED]");
    }

    private static void AppendReportFooter(System.Text.StringBuilder sb, TicketCompanyData company)
    {
        sb.AppendLine("[DASHED]");
        sb.AppendLine(
            $"[CENTER-SMALL]:Impreso: {FormatDateTime(DateTime.Now, includeSeconds: true)}");
        sb.AppendLine("[DASHED]");
        sb.AppendLine($"[CENTER-TINY]:{company.DisplayName} | {company.FooterText}");
        sb.AppendLine($"[CENTER-TINY]:Tel: {company.Phone} | {company.Address}");
    }

    private static System.Text.StringBuilder StartHtml()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine($"<html><head><meta charset='{Charset}'>");
        sb.AppendLine("<meta name='viewport' content='width=device-width,initial-scale=1'>");
        sb.AppendLine("<style>");
        sb.AppendLine("* { margin: 0; padding: 0; box-sizing: border-box; }");
        sb.AppendLine($"html {{ width: {PageWidthMm}mm; }}");
        sb.AppendLine("@media print {");
        sb.AppendLine(
            $"  @page {{ size: {PageWidthMm}mm {PreviewHeightMm}mm; margin: 0; }}");
        sb.AppendLine(
            $"  html, body {{ width: {PageWidthMm}mm; min-width: {PageWidthMm}mm; }}");
        sb.AppendLine("  .no-print { display: none !important; }");
        sb.AppendLine("}");
        AppendCss(sb, "body",
            $"width: {PageWidthMm}mm;",
            $"min-width: {PageWidthMm}mm;",
            "margin: 0;",
            $"padding: {MarginMm}mm;",
            "font-family: 'Courier New', monospace;",
            $"font-size: {FontSize}px;",
            "font-weight: 800;",
            "line-height: 1.28;",
            "color: #000;");
        AppendCss(sb, ".ticket",
            $"width: {ContentWidthMm}mm;",
            $"max-width: {ContentWidthMm}mm;",
            "overflow: hidden;");
        AppendCss(sb, ".center", "text-align: center;");
        AppendCss(sb, ".bold", "font-weight: 800;");
        AppendCss(sb, ".brand",
            "padding: 1px 3px 2px;",
            "text-align: center;");
        AppendCss(sb, ".brand-name",
            "font-size: 10px;",
            "font-weight: 900;",
            "text-transform: uppercase;");
        AppendCss(sb, ".brand-sub",
            "margin-top: 1px;",
            "font-size: 9px;",
            "font-weight: 900;",
            "text-transform: uppercase;");
        AppendCss(sb, ".doc-title",
            "margin: 6px 0 5px;",
            "padding: 2px 0;",
            "text-align: center;",
            "font-size: 13px;",
            "font-weight: 900;",
            "text-transform: uppercase;");
        AppendCss(sb, ".line",
            "border-top: 1px dashed #000;",
            "margin: 5px 0;",
            "height: 0;");
        AppendCss(sb, ".solid-line",
            "border-top: 2px solid #000;",
            "margin: 5px 0;",
            "height: 0;");
        AppendCss(sb, ".row",
            "display: flex;",
            "justify-content: space-between;",
            "gap: 5px;",
            "align-items: baseline;");
        AppendCss(sb, ".row + .row", "margin-top: 2px;");
        AppendCss(sb, ".label",
            "font-size: 10px;",
            "font-weight: 800;",
            "text-transform: uppercase;");
        AppendCss(sb, ".value",
            "font-size: 10px;",
            "text-align: right;",
            "font-weight: 900;",
            "overflow-wrap: anywhere;");
        AppendCss(sb, ".plate-label",
            "font-size: 10px;",
            "font-weight: 900;",
            "text-transform: uppercase;",
            "text-align: center;",
            "margin: 4px 0 2px;");
        AppendCss(sb, ".total-box",
            "margin: 6px 0;",
            "padding: 5px 3px;",
            "border-top: 3px double #000;",
            "border-bottom: 3px double #000;",
            "text-align: center;");
        AppendCss(sb, ".total-label",
            "font-size: 10px;",
            "font-weight: 900;",
            "text-transform: uppercase;");
        AppendCss(sb, ".total-amount",
            "font-size: 22px;",
            "line-height: 1.05;",
            "font-weight: 900;");
        AppendCss(sb, ".small", $"font-size: {SmallSize}px;", "font-weight: 900;");
        AppendCss(sb, ".footer",
            "text-align: center;",
            "font-size: 9px;",
            "font-weight: 900;");
        sb.AppendLine("</style></head><body><div class='ticket'>");
        return sb;
    }

    private static string FinishHtml(System.Text.StringBuilder sb)
    {
        sb.AppendLine("<script>");
        sb.AppendLine(
            "window.onload = function() { setTimeout(function() { window.print(); }, 250); };");
        sb.AppendLine(
            "window.onafterprint = function() { setTimeout(function() { window.close(); }, 150); };");
        sb.AppendLine("</script></div></body></html>");
        return sb.ToString();
    }

    private static void AppendReportHtmlHeader(System.Text.StringBuilder sb,
        TicketCompanyData company, string title)
    {
        sb.AppendLine("<div class='brand'>");
        sb.AppendLine($"<div class='brand-name'>{Html(company.TradeName)}</div>");
        sb.AppendLine(
            $"<div class='brand-sub'>{Html(company.TaxId)} - {Html(company.Address)}</div>");
        sb.AppendLine("</div>");
        sb.AppendLine($"<div class='doc-title'>{Html(title)}</div>");
    }

    private static void AppendHtmlRow(System.Text.StringBuilder sb, string label,
        string value)
    {
        sb.AppendLine("<div class='row'>");
        sb.AppendLine($"<span class='label'>{Html(label)}</span>");
        sb.AppendLine($"<span class='value'>{Html(value)}</span>");
        sb.AppendLine("</div>");
    }

    private static void AppendHtmlRowBold(System.Text.StringBuilder sb, string label,
        string value)
    {
        sb.AppendLine("<div class='row'>");
        sb.AppendLine($"<span class='label' style='font-weight:900;'>{Html(label)}</span>");
        sb.AppendLine($"<span class='value' style='font-weight:900;'>{Html(value)}</span>");
        sb.AppendLine("</div>");
    }

    private static void AppendHtmlLine(System.Text.StringBuilder sb)
    {
        sb.AppendLine("<div class='line'></div>");
    }

    private static void AppendHtmlDouble(System.Text.StringBuilder sb)
    {
        sb.AppendLine("<div class='total-box'>");
    }

    private static void AppendHtmlTotal(System.Text.StringBuilder sb, string amount)
    {
        sb.AppendLine($"<div class='total-amount'>{Html(amount)}</div>");
        sb.AppendLine("</div>");
    }

    private static void AppendHtmlSolid(System.Text.StringBuilder sb)
    {
        sb.AppendLine("<div class='solid-line'></div>");
    }

    private static void AppendReportHtmlFooter(System.Text.StringBuilder sb,
        TicketCompanyData company)
    {
        sb.AppendLine("<div class='line'></div>");
        sb.AppendLine("<div class='footer'>");
        sb.AppendLine(
            $"<div>Impreso: {FormatDateTime(DateTime.Now, includeSeconds: true)}</div>");
        sb.AppendLine("<div class='solid-line'></div>");
        sb.AppendLine("<div class='bold'>GRACIAS POR SU VISITA</div>");
        sb.AppendLine(
            $"<div>{Html(company.DisplayName)} - {Html(company.FooterText)}</div>");
        sb.AppendLine(
            $"<div>Tel: {Html(company.Phone)} - {Html(company.Address)}</div>");
        sb.AppendLine("</div>");
    }

    private static void AppendCss(System.Text.StringBuilder sb, string selector,
        params string[] declarations)
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

    private static string Html(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
    private static string Money(int amount) => $"$ {amount:N0}";

    private static string FormatDateTime(DateTime value, bool includeSeconds = false)
    {
        var format = includeSeconds ? "dd/MM/yyyy hh:mm:ss tt" : "dd/MM/yyyy hh:mm tt";
        return value.ToString(format, CultureInfo.InvariantCulture);
    }

    private static string FormatTime(DateTime value)
    {
        return value.ToString("hh:mm tt", CultureInfo.InvariantCulture);
    }

    public async Task<(string TaggedText, string Html)> GenerateTurnoTicketAsync(
        TurnoReportData data, CancellationToken cancellationToken = default)
    {
        data.Company = await companyInformationService.GetAsync(cancellationToken);
        return (GenerateTurnoText(data), GenerateTurnoHtml(data));
    }

    private static string GenerateTurnoText(TurnoReportData data)
    {
        var sb = new System.Text.StringBuilder();
        AppendReportHeader(sb, data.Company, "ENTREGA DE TURNO");
        sb.AppendLine($"[ROW]:Fecha:|{data.Date:dd/MM/yyyy}");
        sb.AppendLine("[DASHED]");
        if (!string.IsNullOrWhiteSpace(data.OperadorEntrega))
            sb.AppendLine($"[ROW]:Entrega:|{data.OperadorEntrega}");
        if (!string.IsNullOrWhiteSpace(data.OperadorRecibe))
            sb.AppendLine($"[ROW]:Recibe:|{data.OperadorRecibe}");
        sb.AppendLine("[DASHED]");
        sb.AppendLine($"[ROW]:Parqueos activos:|{data.ParqueosActivos}");
        sb.AppendLine($"[ROW]:Lavados entregados:|{data.LavadosEntregados}");
        sb.AppendLine($"[ROW]:Lavados pendientes:|{data.LavadosPendientes}");
        sb.AppendLine("[DOUBLE]");
        sb.AppendLine($"[HUGE]:$ {data.TotalPagos:N0}");
        sb.AppendLine($"[CENTER]:TOTAL PAGOS DIA");
        sb.AppendLine("[DASHED]");

        if (data.Parqueos.Count > 0)
        {
            sb.AppendLine("[CENTER]:PARQUEOS ACTIVOS");
            sb.AppendLine("[DASHED]");
            foreach (var p in data.Parqueos)
                sb.AppendLine($"[ROW]:{p.VehicleType} {p.Placa}|{p.Tiempo} ~$ {p.Estimado:N0}");
            sb.AppendLine("[DASHED]");
        }

        if (data.Lavados.Count > 0)
        {
            sb.AppendLine("[CENTER]:LAVADOS DEL DIA");
            sb.AppendLine("[DASHED]");
            foreach (var l in data.Lavados)
            {
                var pagado = l.Pagado ? "PAG" : "NO PAG";
                sb.AppendLine($"[ROW]:{l.Estado} {l.Placa}|{pagado} $ {l.Precio:N0}");
            }
            sb.AppendLine("[DASHED]");
        }

        sb.AppendLine("[DASHED]");
        sb.AppendLine($"[CENTER]:Firma entrega: ________________");
        sb.AppendLine($"[CENTER]:Firma recibe: ________________");
        AppendReportFooter(sb, data.Company);
        return sb.ToString();
    }

    private static string GenerateTurnoHtml(TurnoReportData data)
    {
        var sb = StartHtml();
        AppendReportHtmlHeader(sb, data.Company, "ENTREGA DE TURNO");
        AppendHtmlRow(sb, "Fecha", data.Date.ToString("dd/MM/yyyy"));
        AppendHtmlLine(sb);
        if (!string.IsNullOrWhiteSpace(data.OperadorEntrega))
            AppendHtmlRow(sb, "Entrega", data.OperadorEntrega);
        if (!string.IsNullOrWhiteSpace(data.OperadorRecibe))
            AppendHtmlRow(sb, "Recibe", data.OperadorRecibe);
        AppendHtmlLine(sb);
        AppendHtmlRow(sb, "Parqueos activos", data.ParqueosActivos.ToString());
        AppendHtmlRow(sb, "Lavados entregados", data.LavadosEntregados.ToString());
        AppendHtmlRow(sb, "Lavados pendientes", data.LavadosPendientes.ToString());
        AppendHtmlDouble(sb);
        AppendHtmlTotal(sb, Money(data.TotalPagos));
        sb.AppendLine("<div class='total-label'>TOTAL PAGOS DIA</div>");
        AppendHtmlLine(sb);

        if (data.Parqueos.Count > 0)
        {
            sb.AppendLine("<div class='plate-label'>PARQUEOS ACTIVOS</div>");
            sb.AppendLine("<div class='line'></div>");
            foreach (var p in data.Parqueos)
                AppendHtmlRow(sb, $"{p.VehicleType} {p.Placa}", $"{p.Tiempo} ~$ {p.Estimado:N0}");
            AppendHtmlLine(sb);
        }

        if (data.Lavados.Count > 0)
        {
            sb.AppendLine("<div class='plate-label'>LAVADOS DEL DIA</div>");
            sb.AppendLine("<div class='line'></div>");
            foreach (var l in data.Lavados)
            {
                var pagado = l.Pagado ? "PAG" : "NO PAG";
                AppendHtmlRow(sb, $"{l.Estado} {l.Placa}", $"{pagado} $ {l.Precio:N0}");
            }
            AppendHtmlLine(sb);
        }

        AppendHtmlLine(sb);
        sb.AppendLine("<div class='center'>Firma entrega: ________________</div>");
        sb.AppendLine("<div style='margin-top:12px;'></div>");
        sb.AppendLine("<div class='center'>Firma recibe: ________________</div>");
        AppendReportHtmlFooter(sb, data.Company);
        return FinishHtml(sb);
    }
}

public sealed class TurnoReportData
{
    public DateTime Date { get; set; }
    public string? OperadorEntrega { get; set; }
    public string? OperadorRecibe { get; set; }
    public int ParqueosActivos { get; set; }
    public int LavadosEntregados { get; set; }
    public int LavadosPendientes { get; set; }
    public int TotalPagos { get; set; }
    public List<TurnoParqueoData> Parqueos { get; set; } = new();
    public List<TurnoLavadoData> Lavados { get; set; } = new();
    public TicketCompanyData Company { get; set; } = TicketCompanyData.Default;
}

public sealed class TurnoParqueoData
{
    public string Placa { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string Tiempo { get; set; } = string.Empty;
    public int Estimado { get; set; }
}

public sealed class TurnoLavadoData
{
    public string Placa { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int Precio { get; set; }
    public bool Pagado { get; set; }
}
