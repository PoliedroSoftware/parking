using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Blazor.Domain.Enums;

public enum GateType
{
    [Description("無")]
    None = 0,

    [Description("入口")]
    Entry = 1,

    [Description("出口")]
    Exit = 2,

    [Description("出入口")]
    EntryExit = Entry | Exit
}
