using System.ComponentModel;

namespace CleanArchitecture.Blazor.Domain.Enums;

public enum VehicleTypes
{
    [Description("Ninguno")]
    None = 0b_0000_0000,

    [Description("Carro")]
    PrivateCar = 1 << 0,

    [Description("Moto")]
    MotorCycle = 1 << 1,

    [Description("Camioneta")]
    LightGoods = 1 << 2,

    [Description("Camion")]
    HeavyGoods = 1 << 3,

    [Description("Bus")]
    Coaches = 1 << 4,

    [Description("Taxi")]
    Containers = 1 << 5,

    [Description("Buseta")]
    LightBuses = 1 << 6,
}
