using CleanArchitecture.Blazor.Domain.Common.Entities;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class CompanyInformation : BaseAuditableEntity
{
    public string DisplayName { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string FooterText { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
