using CleanArchitecture.Blazor.Domain.Common.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class Expense : BaseAuditableEntity, IMayHaveTenant
{
    public DateTime Date { get; set; } = DateTime.Today;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentMethods PaymentMethod { get; set; } = PaymentMethods.Cash;
    public string? Notes { get; set; }
    public string? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}
