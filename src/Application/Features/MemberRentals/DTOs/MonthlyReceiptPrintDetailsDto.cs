#nullable enable
#nullable disable warnings

namespace CleanArchitecture.Blazor.Application.Features.MemberRentals.DTOs;

public class MonthlyReceiptPrintDetailsDto
{
    public int MemberRentalId { get; set; }
    public int? MemberId { get; set; }
    public string? MemberName { get; set; }
    public string? LicensePlate { get; set; }
    public string? CardId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime? PaymentTime { get; set; }
    public PaymentMethods? PaymentMethodId { get; set; }
    public string PaidMonth => StartDate.HasValue
        ? CultureInfo.GetCultureInfo("es-CO").TextInfo.ToTitleCase(
            StartDate.Value.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("es-CO")))
        : string.Empty;
    public List<MonthlyReceiptVehicleDto> Vehicles { get; set; } = new();
    public List<MonthlyReceiptPendingWashDto> PendingWashes { get; set; } = new();
}

public class MonthlyReceiptVehicleDto
{
    public string LicensePlate { get; set; } = string.Empty;
    public VehicleTypes? VehicleType { get; set; }
    public string VehicleTypeDescription => VehicleType?.GetDescription() ?? string.Empty;
}

public class MonthlyReceiptPendingWashDto
{
    public int Id { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public WashServiceType? WashServiceType { get; set; }
    public string WashServiceDescription => WashServiceType?.GetDescription() ?? string.Empty;
    public CarWashStatus? Status { get; set; }
    public string StatusDescription => Status?.GetDescription() ?? string.Empty;
    public decimal Price { get; set; }
}
