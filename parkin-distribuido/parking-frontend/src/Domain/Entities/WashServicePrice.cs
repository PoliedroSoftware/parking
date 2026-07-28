using CleanArchitecture.Blazor.Domain.Common.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class WashServicePrice : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public WashServiceType ServiceType { get; set; } = WashServiceType.None;
    public VehicleTypes VehicleType { get; set; } = VehicleTypes.None;
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
}
