namespace CleanArchitecture.Blazor.Application.Common.Interfaces;

public interface IRemotePrintService
{
    Task<bool> SendToRemotePrinterAsync(string ticketContent, CancellationToken cancellationToken = default);
}
