using CleanArchitecture.Blazor.Domain.Common.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class CarWash : BaseAuditableEntity, IMayHaveTenant
{
    public string LicensePlate { get; set; } = string.Empty;
    public VehicleTypes VehicleType { get; set; } = VehicleTypes.None;
    public WashServiceType WashServiceType { get; set; } = WashServiceType.None;
    public CarWashStatus Status { get; set; } = CarWashStatus.Pending;
    public decimal Price { get; set; }
    public decimal CommissionTotal { get; set; }
    public decimal WeekendSurcharge { get; set; }
    public int QueueNumber { get; set; }
    public DateTime? EstimatedDelivery { get; set; }
    public string? Notes { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsPaid { get; set; } = false;
    public PaymentMethods PaymentMethod { get; set; } = PaymentMethods.None;

    public string? TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public int? ZoneId { get; set; }
    public Zone? Zone { get; set; }

    public int? MemberId { get; set; }
    public Member? Member { get; set; }
    public bool ChargeToMonthly { get; set; } = false;

    public virtual ICollection<CarWashAdditional> Additionals { get; set; } = new List<CarWashAdditional>();
    public virtual ICollection<CarWashOperator> Operators { get; set; } = new List<CarWashOperator>();
}
