using CleanArchitecture.Blazor.Domain.Common.Entities;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class CarWashAdditional : BaseAuditableEntity
{
    public int CarWashId { get; set; }
    public CarWash? CarWash { get; set; }
    public string AdditionalName { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
