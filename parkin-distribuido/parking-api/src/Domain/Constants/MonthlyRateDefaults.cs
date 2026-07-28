using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Domain.Constants;

public static class MonthlyRateDefaults
{
    public const string BaseMarker = "[BASE]";

    public static readonly IReadOnlyList<MonthlyRateDefinition> Rates =
    [
        new("Mensual Carro", VehicleTypes.PrivateCar, 250000, 0, "Tarifa mensual base para carro"),
        new("Mensual Moto", VehicleTypes.MotorCycle, 120000, 0, "Tarifa mensual base para moto"),
        new("Mensual Camioneta", VehicleTypes.LightGoods, 300000, 0, "Tarifa mensual base para camioneta"),
        new("Mensual Taxi", VehicleTypes.Containers, 220000, 0, "Tarifa mensual base para taxi")
    ];

    public static MonthlyRateDefinition? FindRate(VehicleTypes vehicleType)
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

public sealed record MonthlyRateDefinition(
    string Name,
    VehicleTypes VehicleType,
    decimal MonthlyFee,
    decimal Deposit,
    string Description);
