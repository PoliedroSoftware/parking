using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Domain.Common.Entities;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class Holiday : BaseAuditableEntity
{
    public DateTime Date { get; set; }
    public string Name_En { get; set; } = string.Empty;
    public string Name_Tc { get; set; } = string.Empty;

    
}
