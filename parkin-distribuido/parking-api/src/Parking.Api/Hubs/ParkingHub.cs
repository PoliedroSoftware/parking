using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Parking.Api.Hubs;

[Authorize]
public class ParkingHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "parking-operators");
        await base.OnConnectedAsync();
    }

    public async Task NotifyEntry(string plate, string vehicleType, DateTime entryTime)
    {
        await Clients.Group("parking-operators").SendAsync("ParkingEntry", new
        {
            Plate = plate,
            VehicleType = vehicleType,
            EntryTime = entryTime
        });
    }

    public async Task NotifyExit(string plate, int amount, string duration)
    {
        await Clients.Group("parking-operators").SendAsync("ParkingExit", new
        {
            Plate = plate,
            Amount = amount,
            Duration = duration
        });
    }
}
