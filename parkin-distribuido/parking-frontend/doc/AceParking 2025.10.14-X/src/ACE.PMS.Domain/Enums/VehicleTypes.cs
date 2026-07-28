
namespace ACE.PMS.Domain.Enums;

[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VehicleTypes
{
    [DisplayEn("None")]
    [DisplayTc("未知")]
    None = 0b_0000_0000,

    [DisplayEn("PrivateCar")]   // Cars/Vans
    [DisplayTc("私家車")]
    PrivateCar = 1 << 0,        // 0b_0000_0001

    [DisplayEn("MotorCycle")]
    [DisplayTc("電單車")]
    MotorCycle = 1 << 1,        // 0b_0000_0010

    [DisplayEn("LightGoods")]
    [DisplayTc("貨車")]
    LightGoods = 1 << 2,        // 0b_0000_0100

    [DisplayEn("HeavyGoods")]
    [DisplayTc("大貨車")]
    HeavyGoods = 1 << 3,        // 0b_0000_1000

    [DisplayEn("Coaches")]
    [DisplayTc("旅游巴士")]
    Coaches = 1 << 4,           // 0b_0001_0000  Coaches/Buses

    [DisplayEn("Container")]
    [DisplayTc("貨櫃車")]
    Containers = 1 << 5,        // 0b_0010_0000

    [DisplayEn("LightBuses")]
    [DisplayTc("小巴")]
    LightBuses = 1 << 6,        // 0b_0100_0000

    [DisplayEn("Non-Private")]
    [DisplayTc("非私家車")]
    NonPrivateCar = 1 << 7,     // 0b_1000_0000 Non-Private Car
}


//Short Code for Vehicle Type(Optional):
//P - Cars/Vans(私家車)
//M - Motor Cycles(電單車)
//L - Light Goods Vehicles(輕型貨車)
//H - Heavy Goods Vehicle(中重型貨車)
//C - Coaches/Buses(旅遊巴士)
//T - Container Vehicles(貨櫃車)
//B – Light Buses(小型巴士)
//N - Non-Private Car(非私家車)