using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Application.Features.Zones.DTOs;
using CleanArchitecture.Blazor.Domain.Entities;

namespace CleanArchitecture.Blazor.Application.Features.Members.DTOs;

public class MemberVehicleDto
{
    public int Id { get; set; }
    public int? MemberId { get; set; }
    public MemberDto? Member { get; set; }
    public int? VehicleId { get; set; }
    public VehicleDto? Vehicle { get; set; }
    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<MemberVehicle, MemberVehicleDto>(MemberList.None);

            CreateMap<MemberVehicleDto, MemberVehicle>(MemberList.None);

            
        }
    }
}
