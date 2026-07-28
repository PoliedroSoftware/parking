using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Domain.Common.Entities;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class SpaceGroup : BaseAuditableEntity
{
    public string Name { get; set; } // Monthly Group Name    
    public int Capacity { get; set; } = 1; // Number of currently assigned members    
    public int? ZoneId { get; set; } // 所屬停車場區域 Zone (如: Main Zone, Car Park 1/F, Loading Bay Area ...)
    public Zone? Zone { get; set; }
    public string? Description { get; set; } // Description    
    public virtual ICollection<Member>? Members { get; set; } // Members assigned to this Space Group
  
}
