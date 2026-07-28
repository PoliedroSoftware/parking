using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Domain.Constants;

public static class WashCatalogDefaults
{
    public const string BaseMarker = "[BASE]";

    public static readonly IReadOnlyList<WashServicePriceDefinition> ServicePrices =
    [
        new("Basico Carro", WashServiceType.Basic, VehicleTypes.PrivateCar, 25000, "Lavado basico para carro"),
        new("Basico Moto", WashServiceType.Basic, VehicleTypes.MotorCycle, 12000, "Lavado basico para moto"),
        new("Basico Camioneta", WashServiceType.Basic, VehicleTypes.LightGoods, 30000, "Lavado basico para camioneta"),
        new("Basico Taxi", WashServiceType.Basic, VehicleTypes.Containers, 20000, "Lavado basico para taxi"),
        new("Basico Camion", WashServiceType.Basic, VehicleTypes.HeavyGoods, 40000, "Lavado basico para camion"),
        new("Basico Bus", WashServiceType.Basic, VehicleTypes.Coaches, 50000, "Lavado basico para bus"),
        new("Premium Carro", WashServiceType.Premium, VehicleTypes.PrivateCar, 45000, "Lavado premium para carro"),
        new("Premium Moto", WashServiceType.Premium, VehicleTypes.MotorCycle, 20000, "Lavado premium para moto"),
        new("Premium Camioneta", WashServiceType.Premium, VehicleTypes.LightGoods, 55000, "Lavado premium para camioneta"),
        new("Full Detail Carro", WashServiceType.FullDetail, VehicleTypes.PrivateCar, 80000, "Detalle completo para carro"),
        new("Full Detail Camioneta", WashServiceType.FullDetail, VehicleTypes.LightGoods, 95000, "Detalle completo para camioneta"),
        new("Carroceria Carro", WashServiceType.BodyOnly, VehicleTypes.PrivateCar, 18000, "Lavado solo carroceria"),
        new("Tapiceria Carro", WashServiceType.Upholstery, VehicleTypes.PrivateCar, 35000, "Limpieza de tapiceria para carro"),
        new("Tapiceria Camioneta", WashServiceType.Upholstery, VehicleTypes.LightGoods, 45000, "Limpieza de tapiceria para camioneta")
    ];

    public static readonly IReadOnlyList<WashAdditionalDefinition> Additionals =
    [
        new("Lavado de Motor", 15000, "Lavado completo del motor con desengrasante"),
        new("Cera Liquida", 10000, "Aplicacion de cera liquida para brillo"),
        new("Aspirado Interior", 8000, "Aspirado completo de tapetes y sillas"),
        new("Desinfeccion Ozono", 20000, "Tratamiento con ozono para eliminar olores"),
        new("Lavado de Chasis", 12000, "Lavado a presion del chasis y bajos"),
        new("Polichado", 25000, "Polichado y abrillantado de pintura")
    ];

    public static bool IsBaseServicePrice(WashServiceType serviceType, VehicleTypes vehicleType)
    {
        return ServicePrices.Any(x => x.ServiceType == serviceType && x.VehicleType == vehicleType);
    }

    public static WashServicePriceDefinition? FindServicePrice(WashServiceType serviceType, VehicleTypes vehicleType)
    {
        return ServicePrices.FirstOrDefault(x => x.ServiceType == serviceType && x.VehicleType == vehicleType);
    }

    public static bool IsBaseAdditional(string? name)
    {
        return Additionals.Any(x => string.Equals(x.Name, name?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public static WashAdditionalDefinition? FindAdditional(string? name)
    {
        return Additionals.FirstOrDefault(x => string.Equals(x.Name, name?.Trim(), StringComparison.OrdinalIgnoreCase));
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

public sealed record WashServicePriceDefinition(
    string Name,
    WashServiceType ServiceType,
    VehicleTypes VehicleType,
    decimal BasePrice,
    string Description);

public sealed record WashAdditionalDefinition(
    string Name,
    decimal Price,
    string Description);
