
namespace ACE.PMS.Domain.Enums;

[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ServiceCategories
{
    [DisplayEn("Hourly")]
    [DisplayTc("時租")]
    Hourly = 1 << 0,    // 1  0b_0000_0001

    [DisplayEn("Monthly")]
    [DisplayTc("月租")]
    Monthly = 1 << 1,   // 2, 0b_0000_0010
}