using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Domain.Common.Entities;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class MemberVehicle:IEntity<int>
{
    public int Id { get; set; }
    public int? MemberId { get; set; }
    public Member? Member { get; set; }
    public int? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
}
