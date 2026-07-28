using Microsoft.AspNetCore.SignalR;

namespace Parking.Api.Hubs;

public class PrintHub : Hub
{
    private static readonly HashSet<string> ConnectedPrinters = new();

    public override async Task OnConnectedAsync()
    {
        ConnectedPrinters.Add(Context.ConnectionId);
        await Clients.Caller.SendAsync("Registered", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        ConnectedPrinters.Remove(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task PrintResult(string printerName, bool success, string? error)
    {
        await Clients.Others.SendAsync("PrintCompleted", new { printerName, success, error });
    }

    public static bool HasPrinters => ConnectedPrinters.Count > 0;
}

public record PrintJob(string PrinterName, string Content, string? JobId = null);
