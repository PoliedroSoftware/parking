using Microsoft.AspNetCore.DataProtection;

namespace CleanArchitecture.Blazor.Server.UI.Services;

public sealed class ParkingQrTokenService(IDataProtectionProvider dataProtectionProvider)
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("CleanArchitecture.Blazor.ParkingExitQr.v1");

    public string Create(int parkingRecordId)
    {
        if (parkingRecordId <= 0)
            throw new ArgumentOutOfRangeException(nameof(parkingRecordId));

        return _protector.Protect(parkingRecordId.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    public bool TryGetParkingRecordId(string? token, out int parkingRecordId)
    {
        parkingRecordId = 0;
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            var value = _protector.Unprotect(token.Trim());
            return int.TryParse(value, out parkingRecordId) && parkingRecordId > 0;
        }
        catch (Exception) when (true)
        {
            parkingRecordId = 0;
            return false;
        }
    }
}
