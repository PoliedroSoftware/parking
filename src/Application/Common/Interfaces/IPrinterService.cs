namespace CleanArchitecture.Blazor.Application.Common.Interfaces;

public interface IPrinterService
{
    string PrinterName { get; set; }
    bool Enabled { get; set; }

    IReadOnlyList<string> GetInstalledPrinters();
    Task<int> ClearQueueAsync(CancellationToken cancellationToken = default);
    Task<int> RepairQueueBeforePrintAsync(CancellationToken cancellationToken = default);
    Task PrintTicketAsync(string ticketContent, CancellationToken cancellationToken = default);
}
