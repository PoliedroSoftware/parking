using CleanArchitecture.Blazor.Domain.Common.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class ParkingRate : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public VehicleTypes VehicleType { get; set; } = VehicleTypes.None;
    public decimal HourlyRate { get; set; }
    public decimal DayRate { get; set; }
    public decimal NightRate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
}
