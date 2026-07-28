using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Domain.Constants;

public static class ParkingRateDefaults
{
    public const string BaseMarker = "[BASE]";

    public static readonly IReadOnlyList<ParkingRateDefinition> Rates =
    [
        new("Carro", VehicleTypes.PrivateCar, 3500, 25000, 12000, "Tarifa base para carro"),
        new("Moto", VehicleTypes.MotorCycle, 2000, 15000, 8000, "Tarifa base para moto"),
        new("Camioneta", VehicleTypes.LightGoods, 4000, 30000, 15000, "Tarifa base para camioneta"),
        new("Taxi", VehicleTypes.Containers, 3000, 20000, 10000, "Tarifa base para taxi")
    ];

    public static ParkingRateDefinition? FindRate(VehicleTypes vehicleType)
    {
        return Rates.FirstOrDefault(x => x.VehicleType == vehicleType);
    }

    public static bool IsMarkedBase(string? description)
    {
        return description?.TrimStart().StartsWith(BaseMarker, StringComparison.OrdinalIgnoreCase) == true;
    }

    public static string MarkDescription(string description)
    {
        var cleanDescription = CleanDescription(description);
        return $"{BaseMarker} {cleanDescription}".Trim();
    }

    public static string CleanDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return string.Empty;

        var trimmed = description.Trim();
        return trimmed.StartsWith(BaseMarker, StringComparison.OrdinalIgnoreCase)
            ? trimmed[BaseMarker.Length..].Trim()
            : trimmed;
    }
}

public sealed record ParkingRateDefinition(
    string Name,
    VehicleTypes VehicleType,
    decimal HourlyRate,
    decimal DayRate,
    decimal NightRate,
    string Description);
