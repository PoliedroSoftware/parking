using System.Drawing.Printing;
using System.Drawing;
using System.Text.Json;

namespace CleanArchitecture.Blazor.Server.UI.Services;

public class PrinterService
{
    private readonly string _configPath;
    private PrinterConfig _config = new();

    public PrinterService(IWebHostEnvironment env)
    {
        _configPath = Path.Combine(env.ContentRootPath, "printer-config.json");
        LoadConfig();
    }

    public string PrinterName
    {
        get => _config.PrinterName;
        set { _config.PrinterName = value; SaveConfig(); }
    }

    public bool Enabled
    {
        get => _config.Enabled;
        set { _config.Enabled = value; SaveConfig(); }
    }

    public List<string> GetInstalledPrinters()
    {
        var printers = new List<string>();
        foreach (string printer in PrinterSettings.InstalledPrinters)
            printers.Add(printer);
        return printers;
    }

    public Task PrintTicketAsync(string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(_config.PrinterName))
            throw new InvalidOperationException("No se ha configurado una impresora.");

        var tcs = new TaskCompletionSource<bool>();
        var printDoc = new PrintDocument();

        printDoc.PrinterSettings.PrinterName = _config.PrinterName;
        printDoc.DefaultPageSettings.PaperSize = new PaperSize("80mm", 283, 6000);
        printDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
        printDoc.DocumentName = "Poliedro Ticket";

        var contentToPrint = htmlContent;

        printDoc.PrintPage += (sender, e) =>
        {
            if (e.Graphics == null) return;
            DrawTicket(e.Graphics, contentToPrint, e.MarginBounds);
            e.HasMorePages = false;
        };

        printDoc.EndPrint += (sender, e) =>
        {
            tcs.TrySetResult(true);
            printDoc.Dispose();
        };

        printDoc.Print();
        return tcs.Task;
    }

    private static void DrawTicket(Graphics g, string content, RectangleF bounds)
    {
        var font = new Font("Courier New", 8, FontStyle.Regular);
        var boldFont = new Font("Courier New", 8, FontStyle.Bold);
        var titleFont = new Font("Courier New", 11, FontStyle.Bold);
        var smallFont = new Font("Courier New", 6.5f, FontStyle.Regular);

        float y = 3;
        float leftMargin = 3;
        float width = bounds.Width;

        var lines = content.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line)) { y += 4; continue; }

            var cleanLine = line.Trim();

            if (cleanLine == "---" || cleanLine.StartsWith("---"))
            {
                g.DrawString(new string('-', 42), font, Brushes.Black, leftMargin, y);
                y += 10;
                continue;
            }

            if (cleanLine.Contains("POLIEDRO") || cleanLine.Contains("SOFTWARE"))
            {
                var sz = g.MeasureString(cleanLine, titleFont);
                g.DrawString(cleanLine, titleFont, Brushes.Black, (width - sz.Width) / 2, y);
                y += 16;
            }
            else if (cleanLine.Contains("TICKET") || cleanLine.Contains("COMPROBANTE") || cleanLine.Contains("ENTRADA") || cleanLine.Contains("SALIDA") || cleanLine.Contains("LAVADO") || cleanLine.Contains("MENSUALIDAD"))
            {
                var sz = g.MeasureString(cleanLine, boldFont);
                g.DrawString(cleanLine, boldFont, Brushes.Black, (width - sz.Width) / 2, y);
                y += 14;
            }
            else if (cleanLine.Contains("TOTAL"))
            {
                g.DrawString(cleanLine, boldFont, Brushes.Black, leftMargin, y);
                y += 14;
            }
            else if (cleanLine.Contains("Gracias") || cleanLine.Contains("visita"))
            {
                var sz = g.MeasureString(cleanLine, smallFont);
                g.DrawString(cleanLine, smallFont, Brushes.Black, (width - sz.Width) / 2, y);
                y += 10;
            }
            else
            {
                g.DrawString(cleanLine, font, Brushes.Black, leftMargin, y);
                y += 12;
            }

            if (y > bounds.Height - 10) break;
        }
    }

    private void LoadConfig()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                _config = JsonSerializer.Deserialize<PrinterConfig>(json) ?? new PrinterConfig();
            }
        }
        catch { _config = new PrinterConfig(); }
    }

    private void SaveConfig()
    {
        try
        {
            var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);
        }
        catch { }
    }

    private class PrinterConfig
    {
        public string PrinterName { get; set; } = string.Empty;
        public bool Enabled { get; set; }
    }
}
