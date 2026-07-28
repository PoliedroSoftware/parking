using System.Net.Http.Json;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Blazor.Infrastructure.Services;

public class RemotePrintService(HttpClient httpClient, ILogger<RemotePrintService> logger) : IRemotePrintService
{
    public async Task<bool> SendToRemotePrinterAsync(string ticketContent, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new { printerName = "POS-80", content = ticketContent };
            var response = await httpClient.PostAsJsonAsync("/api/print", payload, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Ticket enviado a impresora remota via PrintHub");
                return true;
            }

            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning("PrintHub rechazo el ticket: {Error}", error);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "No se pudo conectar al PrintHub remoto");
            return false;
        }
    }
}
