using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Server.UI.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.JSInterop;

namespace CleanArchitecture.Blazor.Server.UI.Services;

public sealed class PosTicketPrintResult
{
    public bool PrintedDirectly { get; private init; }
    public bool PreviewOpened { get; private init; }
    public string Message { get; private init; } = string.Empty;

    public static PosTicketPrintResult Direct(string message) =>
        new() { PrintedDirectly = true, Message = message };

    public static PosTicketPrintResult Preview(string message) =>
        new() { PreviewOpened = true, Message = message };

    public static PosTicketPrintResult Failed(string message) =>
        new() { Message = message };
}

public sealed class PosTicketPrintService(
    TicketService ticketService,
    IPrinterService printerService,
    IJSRuntime jsRuntime,
    CompanyInformationService companyInformationService,
    IHubContext<PrintHub> printHubContext)
{
    public async Task<PosTicketPrintResult> PrintAsync(TicketData ticketData, CancellationToken cancellationToken = default)
    {
        if (ticketData is null)
            return PosTicketPrintResult.Failed("No se recibio informacion del ticket.");

        await companyInformationService.ApplyAsync(ticketData, cancellationToken);
        var content = ticketService.GenerateTicketText(ticketData);

        if (string.IsNullOrWhiteSpace(printerService.PrinterName))
        {
            if (await TryRemotePrintAsync(content))
                return PosTicketPrintResult.Direct("Ticket enviado a impresora remota via PrintHub.");

            await OpenBrowserPreviewAsync(ticketData);
            return PosTicketPrintResult.Preview("Se abrio la vista previa.");
        }

        try
        {
            await printerService.PrintTicketAsync(content, cancellationToken);
            return PosTicketPrintResult.Direct("Ticket enviado directo a la impresora POS.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (await TryRemotePrintAsync(content))
                return PosTicketPrintResult.Direct("Impresion directa fallo. Ticket enviado a impresora remota.");

            var cleanupMessage = await TryCleanupQueueAsync(cancellationToken);
            return PosTicketPrintResult.Failed($"No se pudo imprimir: {ex.Message}{cleanupMessage}");
        }
    }

    public async Task<PosTicketPrintResult> PrintReportAsync(
        string taggedContent, string htmlContent,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(taggedContent))
            return PosTicketPrintResult.Failed("No se genero contenido para el reporte.");

        if (string.IsNullOrWhiteSpace(printerService.PrinterName))
        {
            if (await TryRemotePrintAsync(taggedContent))
                return PosTicketPrintResult.Direct("Reporte enviado a impresora remota via PrintHub.");

            await OpenReportBrowserPreviewAsync(htmlContent);
            return PosTicketPrintResult.Preview("Se abrio la vista previa.");
        }

        try
        {
            await printerService.PrintTicketAsync(taggedContent, cancellationToken);
            return PosTicketPrintResult.Direct("Reporte enviado directo a la impresora POS.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (await TryRemotePrintAsync(taggedContent))
                return PosTicketPrintResult.Direct("Impresion directa fallo. Reporte enviado a impresora remota.");

            var cleanupMessage = await TryCleanupQueueAsync(cancellationToken);
            return PosTicketPrintResult.Failed($"No se pudo imprimir: {ex.Message}{cleanupMessage}");
        }
    }

    private async Task<bool> TryRemotePrintAsync(string content)
    {
        try
        {
            if (!PrintHub.HasPrinters) return false;
            await printHubContext.Clients.All.SendAsync("PrintJob", new
            {
                PrinterName = "POS-80",
                Content = content,
                JobId = Guid.NewGuid().ToString("N")[..8]
            });
            return true;
        }
        catch { return false; }
    }

    private async Task OpenBrowserPreviewAsync(TicketData ticketData)
    {
        await jsRuntime.InvokeVoidAsync("openTicketWindow", ticketService.GenerateTicketHtmlForWindow(ticketData));
    }

    private async Task OpenReportBrowserPreviewAsync(string htmlContent)
    {
        await jsRuntime.InvokeVoidAsync("openTicketWindow", htmlContent);
    }

    private async Task<string> TryCleanupQueueAsync(CancellationToken cancellationToken)
    {
        try
        {
            var repairedJobs = await printerService.RepairQueueBeforePrintAsync(cancellationToken);
            return repairedJobs > 0
                ? $" Se limpiaron {repairedJobs} trabajo(s) bloqueado(s) de la cola."
                : string.Empty;
        }
        catch { return " No se pudo limpiar la cola automaticamente."; }
    }
}
