

using System.ComponentModel;

namespace ACE.PMS.Domain.Enums;

[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GateType
{
    [Description("無")]
    None = 0,

    [Description("入口")]
    Entry=1,

    [Description("出口")]
    Exit=2,

    [Description("出入口")]
    EntryExit = Entry | Exit
}