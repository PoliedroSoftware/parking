using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Application.Features.Carparks.DTOs;
using CleanArchitecture.Blazor.Application.Features.Charges.DTOs;
using CleanArchitecture.Blazor.Application.Features.Members.DTOs;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Application.Features.Zones.DTOs;

public class VehicleDto
{
    public int Id { get; set; }
    public int? MemberId { get; set; }
    public MemberDto? Member { get; set; }
    // 車類名稱 Vehicle Type Name
    public string Name { get; set; } = string.Empty;

    // 服務類別 Service Category
    public ServiceCategories ServiceCategoryId { get; set; } = ServiceCategories.Hourly;

    // 車輛類型 Vehicle Type
    public VehicleTypes VehicleTypeId { get; set; } = VehicleTypes.PrivateCar;

    // 所屬停車場區域 Zone (如: Main Zone, Car Park 1/F, Loading Bay Area ...)    
    public int? ZoneId { get; set; } // Foreign Key
    public ZoneDto? Zone { get; set; } // Required Reference Navigation Property

    // 收費類型 Charge Type
    public int? ChargeId { get; set; } // Optional foreign key to Charge(principal)
    public ChargeDto? Charge { get; set; } // Optional reference navigation to Charge(principal)


    // 可泊車位數 Parking Capacity
    public int Capacity { get; set; } = 100;

    // 本車類別的車位數已滿時，是否允許該類別的車輛繼續進入泊車場 (如: 電單車位滿，但總車位未滿，則仍可進入) 
    // If the parking spaces for this vehicle type are full, are vehicles of this type allowed to continue to enter the car park
    // (e.g. if motorcycle spaces are full but total spaces are not full, entry is still allowed)
    public bool AllowEntryWhenFull;

    // 是否人工暫停進入 Whether to manually suspend entry
    public bool ManualFull { get; set; }

    // 車牌號是否可被車牌自動識別系統(LPR)識別 Whether the license plate number can be recognized by the automatic license plate recognition system (LPR).
    public bool CanRecognizePlate { get; set; } = true;

    // 是否啟用 Is Active
    public bool IsActive { get; set; } = true;

    //[NotMapped] 
    public int Occupied { get; set; } //已使用泊車位數量

    //[NotMapped] 
    public int Available => (Capacity - Occupied) < 0 ? 0 : (Capacity - Occupied); //可用泊車位數量


    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Vehicle, VehicleDto>(MemberList.None);
            
            CreateMap<VehicleDto, Vehicle>(MemberList.None)
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedAt, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedById, opt => opt.Ignore())
            .ForMember(dest => dest.DomainEvents, opt => opt.Ignore());
        }
    }
}
