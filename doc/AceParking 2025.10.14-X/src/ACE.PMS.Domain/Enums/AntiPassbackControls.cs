using System.ComponentModel;

namespace ACE.PMS.Domain.Enums;

[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AntiPassbackControls
{
    [Description("不查驗")]
    None = 0,
    
    [Description("入口查驗")]
    Entry =    1 << 0,    // 1  0b_0000_0001,

    [Description("出口查驗")]
    Exit =     1 << 1,    // 2  0b_0000_0010,

    [Description("出入口查驗")]
    EntryExit = 1 << 2,    // 4  0b_0000_0100,
}