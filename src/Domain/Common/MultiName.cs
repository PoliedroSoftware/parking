using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Blazor.Domain.Common;

public class MultiName {
    public string? En { get; set; }
    public string? Tc { get; set; }
    public MultiName(string en, string tc)
    {
        En = en;
        Tc = tc;
    }
}
public class MultiCodeName {
    public string? Code { get; set; }
    public string? En { get; set; }
    public string? Tc { get; set; }
    public MultiCodeName(string code, string en, string tc)
    {
        Code = code;
        En = en;
        Tc = tc;
    }
}
