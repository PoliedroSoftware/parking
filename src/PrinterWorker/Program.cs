using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Text;

if (args.Length < 3)
{
    Console.Error.WriteLine("Uso: PrinterWorker.exe <printerName> <contentFile> <outputFile>");
    return 1;
}

var printerName = args[0];
var contentFile = args[1];
var outputFile = args[2];

try
{
    var content = File.ReadAllText(contentFile, Encoding.UTF8);

    using var printDoc = new PrintDocument();
    printDoc.PrinterSettings.PrinterName = printerName;
    printDoc.PrintController = new StandardPrintController();
    printDoc.DefaultPageSettings.Margins = new Margins(1, 1, 0, 0);
    printDoc.DocumentName = "Poliedro";

    var done = new ManualResetEvent(false);
    Exception? printError = null;

    printDoc.PrintPage += (sender, e) =>
    {
        try
        {
            if (e?.Graphics != null)
            {
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                DrawTicket(e.Graphics, content, e.MarginBounds);
            }
        }
        catch (Exception ex) { printError = ex; }
        e!.HasMorePages = false;
    };

    printDoc.EndPrint += (_, _) => done.Set();

    printDoc.Print();
    done.WaitOne(TimeSpan.FromSeconds(30));

    if (printError != null)
    {
        File.WriteAllText(outputFile, printError.ToString());
        return 1;
    }

    File.WriteAllText(outputFile, "OK");
    return 0;
}
catch (Exception ex)
{
    File.WriteAllText(outputFile, ex.ToString());
    return 1;
}

static void DrawTicket(Graphics g, string content, RectangleF bounds)
{
    var lines = content.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
    float w = bounds.Width;
    float y = 0;
    float lm = 2;

    using var regular = new Font("Courier New", 8.5f, FontStyle.Regular);
    using var bold = new Font("Courier New", 8.5f, FontStyle.Bold);
    using var titleFont = new Font("Courier New", 11f, FontStyle.Bold);
    using var hugeFont = new Font("Courier New", 22f, FontStyle.Bold);
    using var small = new Font("Courier New", 6.5f, FontStyle.Regular);
    using var smallBold = new Font("Courier New", 6.5f, FontStyle.Bold);

    foreach (var line in lines)
    {
        var clean = line.Trim();
        if (string.IsNullOrEmpty(clean)) { y += 2; continue; }

        if (clean == "[HEADER]")
        {
            var rect = new RectangleF(0, y, w, 26);
            g.FillRectangle(Brushes.Black, rect);
            var sz = g.MeasureString("POLIEDRO SOFTWARE", titleFont);
            g.DrawString("POLIEDRO SOFTWARE", titleFont, Brushes.White, (w - sz.Width) / 2, y + 4);
            y += 28; continue;
        }

        if (clean == "[DIVIDER]" || clean == "[DASHED]" || clean == "[DOUBLE]")
        {
            g.DrawLine(Pens.Black, lm, y + 3, w - lm, y + 3);
            y += 7; continue;
        }

        if (clean.StartsWith("[HUGE]:"))
        {
            var text = clean[7..];
            var sz = g.MeasureString(text, hugeFont);
            g.DrawString(text, hugeFont, Brushes.Black, (w - sz.Width) / 2, y);
            y += 28; continue;
        }

        if (clean.StartsWith("[CENTER]:"))
        {
            var text = clean[9..];
            var sz = g.MeasureString(text, bold);
            g.DrawString(text, bold, Brushes.Black, (w - sz.Width) / 2, y);
            y += 13; continue;
        }

        if (clean.StartsWith("[CENTER-SMALL]:"))
        {
            var text = clean[15..];
            var sz = g.MeasureString(text, small);
            g.DrawString(text, small, Brushes.Black, (w - sz.Width) / 2, y);
            y += 9; continue;
        }

        if (clean.StartsWith("[CENTER-TINY]:"))
        {
            var text = clean[14..];
            var sz = g.MeasureString(text, smallBold);
            g.DrawString(text, smallBold, Brushes.Black, (w - sz.Width) / 2, y);
            y += 9; continue;
        }

        if (clean.StartsWith("[ROW]:"))
        {
            var parts = clean[5..].Split('|');
            if (parts.Length == 2)
            {
                g.DrawString(parts[0].Trim(), regular, Brushes.Black, lm, y);
                var rsz = g.MeasureString(parts[1].Trim(), regular);
                g.DrawString(parts[1].Trim(), regular, Brushes.Black, w - rsz.Width - lm, y);
            }
            y += 13; continue;
        }

        if (clean.StartsWith("[ROW-BOLD]:"))
        {
            var parts = clean[10..].Split('|');
            if (parts.Length == 2)
            {
                g.DrawString(parts[0].Trim(), bold, Brushes.Black, lm, y);
                var rsz = g.MeasureString(parts[1].Trim(), bold);
                g.DrawString(parts[1].Trim(), bold, Brushes.Black, w - rsz.Width - lm, y);
            }
            y += 13; continue;
        }

        g.DrawString(clean, regular, Brushes.Black, lm, y);
        y += 12;
    }
}
