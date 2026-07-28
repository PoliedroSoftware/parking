using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Blazor.Domain.Enums;

public enum MonthlyConversions
{

    [Description("無")]
    None = 0,

    [Description("無效轉時租")]
    Invalid = 1,

    [Description("過期轉時租")]
    Expired = 2,

    [Description("額度用盡轉時租")]
    GroupFulled = 4,
}