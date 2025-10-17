using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Blazor.Application.Features.Zones.DTOs;

public class SpaceGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Space Group Name
    public int Capacity { get; set; } = 1; // Number of currently assigned members    
    public int? ZoneId { get; set; } // 所屬停車場區域 Zone (如: Main Zone, Car Park 1/F, Loading Bay Area ...)
    public ZoneDto? Zone { get; set; }
    public string? Description { get; set; } // Description    
    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<SpaceGroup, SpaceGroupDto>(MemberList.None);
            CreateMap<SpaceGroupDto, SpaceGroup>(MemberList.None)
                .ForMember(dest => dest.Zone, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedAt, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedById, opt => opt.Ignore())
            .ForMember(dest => dest.DomainEvents, opt => opt.Ignore());
        }
    }
}
