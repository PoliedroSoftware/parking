using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Blazor.Domain.Enums;

public enum ServiceCategories
{
    [DisplayEn("Hourly")]
    [DisplayTc("時租")]
    Hourly = 1 << 0,    // 1  0b_0000_0001

    [DisplayEn("Monthly")]
    [DisplayTc("月租")]
    Monthly = 1 << 1,   // 2, 0b_0000_0010
}
