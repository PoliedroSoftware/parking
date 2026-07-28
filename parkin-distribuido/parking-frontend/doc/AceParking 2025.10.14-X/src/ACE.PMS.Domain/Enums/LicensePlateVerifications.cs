
using System.ComponentModel;

namespace ACE.PMS.Domain.Enums;

[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LicensePlateVerifications
{
    [Description("不驗證")]
    None = 0,

    [Description("入口驗證")]
    Entry = 1 << 0,     // 1  0b_0000_0001, 入口查驗登記車牌

    [Description("出口驗證")]
    Exit =  1 << 1,     // 2  0b_0000_0010, 出口查驗登記車牌

    [Description("出入口驗證")]    
    EntryExit= Entry | Exit, // 3  0b_0000_0011, 出入出口查驗登記車牌
}
