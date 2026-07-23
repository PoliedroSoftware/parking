namespace CleanArchitecture.Blazor.Application.Common.Security;

public static partial class Permissions
{
    [DisplayName("Parking Payment Permissions")]
    [Description("Set permissions for parking payment adjustments.")]
    public static class ParkingPayments
    {
        [Description("Allows adjusting the paid amount of a completed parking record.")]
        public const string Adjust = "Permissions.ParkingPayments.Adjust";
    }
}

public class ParkingPaymentsAccessRights
{
    public bool Adjust { get; set; }
}
