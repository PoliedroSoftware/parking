using CleanArchitecture.Blazor.Domain.Common.Entities;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class ParkingRecord : BaseAuditableEntity
{
    public string LicensePlate { get; set; } = string.Empty;
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Activo";
    public string? TicketNumber { get; set; }
    public string? CustomerName { get; set; }
    public string? Notes { get; set; }
}
