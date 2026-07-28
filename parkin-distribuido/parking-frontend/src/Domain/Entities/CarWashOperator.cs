using CleanArchitecture.Blazor.Domain.Common.Entities;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class CarWashOperator : BaseAuditableEntity
{
    public int CarWashId { get; set; }
    public CarWash? CarWash { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public decimal Commission { get; set; }
}
