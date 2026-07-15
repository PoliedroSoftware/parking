using System.Collections.Concurrent;

namespace CleanArchitecture.Blazor.Server.UI.Services;

public class ParkingSession
{
    public string LicensePlate { get; set; } = string.Empty;
    public DateTime EntryTime { get; set; }
    public string VehicleType { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;
}

public class ParkingSessionService
{
    private readonly ConcurrentDictionary<string, ParkingSession> _activeSessions = new(StringComparer.OrdinalIgnoreCase);

    public bool IsParked(string licensePlate)
    {
        return _activeSessions.ContainsKey(licensePlate);
    }

    public ParkingSession? GetSession(string licensePlate)
    {
        _activeSessions.TryGetValue(licensePlate, out var session);
        return session;
    }

    public void RegisterEntry(ParkingSession session)
    {
        _activeSessions[session.LicensePlate] = session;
    }

    public ParkingSession? Checkout(string licensePlate)
    {
        _activeSessions.TryRemove(licensePlate, out var session);
        return session;
    }

    public int ActiveSessionCount => _activeSessions.Count;

    public IReadOnlyCollection<ParkingSession> ActiveSessions => _activeSessions.Values.ToList().AsReadOnly();
}
