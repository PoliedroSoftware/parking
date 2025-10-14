
using System.ComponentModel;

namespace ACE.PMS.Domain.Enums;

[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SpaceTypes
{    
    [Description("普通")]
    Regular = 0,
        
    [Description("固定")]
    Reserved = 1 << 0,

    [Description("浮動")]
    Floating = 1 << 1,

    [Description("有蓋")]
    Covered = 1 << 2,

    [Description("露天")]
    Open = 1 << 3,

    [Description("固定有蓋")]
    ReservedCovered = Reserved | Covered,

    [Description("固定露天")]
    ReservedOpen = Reserved | Open,

    [Description("浮動有蓋")]
    FloatingCovered = Floating | Covered,

    [Description("浮動露天")]
    FloatingOpen = Floating | Open,
}