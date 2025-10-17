using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Domain.Common.Entities;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class ZoneMember: BaseAuditableEntity
{
    public int? ZoneId { get; set; }
    public Zone? Zone { get; set; } = null!; 
    public int? MemberId { get; set; }
    public Member? Member { get; set; } = null!;
}
