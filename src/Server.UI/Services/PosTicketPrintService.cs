using CleanArchitecture.Blazor.Application.Common.Interfaces;
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
    CompanyInformationService companyInformationService)
{
    public async Task<PosTicketPrintResult> PrintAsync(TicketData ticketData, CancellationToken cancellationToken = default)
    {
        if (ticketData is null)
            return PosTicketPrintResult.Failed("No se recibio informacion del ticket.");

        await companyInformationService.ApplyAsync(ticketData, cancellationToken);

        if (string.IsNullOrWhiteSpace(printerService.PrinterName))
        {
            await OpenBrowserPreviewAsync(ticketData);
            return PosTicketPrintResult.Preview(
                "No hay impresora POS configurada. Se abrio la vista previa de impresion.");
        }

        try
        {
            await printerService.PrintTicketAsync(ticketService.GenerateTicketText(ticketData), cancellationToken);
            return PosTicketPrintResult.Direct("Ticket enviado directo a la impresora POS.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var cleanupMessage = await TryCleanupQueueAsync(cancellationToken);
            return PosTicketPrintResult.Failed(
                $"No se pudo imprimir directo en POS: {ex.Message}{cleanupMessage}");
        }
    }

    private async Task OpenBrowserPreviewAsync(TicketData ticketData)
    {
        await jsRuntime.InvokeVoidAsync("openTicketWindow", ticketService.GenerateTicketHtmlForWindow(ticketData));
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
        catch
        {
            return " No se pudo limpiar la cola automaticamente.";
        }
    }
}
