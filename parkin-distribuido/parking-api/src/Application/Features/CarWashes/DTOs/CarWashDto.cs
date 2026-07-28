#nullable enable
#nullable disable warnings

using CleanArchitecture.Blazor.Application.Features.Members.DTOs;
using CleanArchitecture.Blazor.Application.Features.Tenants.DTOs;
using CleanArchitecture.Blazor.Application.Features.Zones.DTOs;
using CleanArchitecture.Blazor.Domain.Entities;

namespace CleanArchitecture.Blazor.Application.Features.CarWashes.DTOs;

[Description("Lavados")]
public class CarWashDto
{
    [Description("Id")]
    public int Id { get; set; }
    [Description("Placa")]
    public string? LicensePlate { get; set; }
    [Description("Tipo de vehiculo")]
    public VehicleTypes? VehicleType { get; set; }
    [Description("Tipo de lavado")]
    public WashServiceType? WashServiceType { get; set; }
    [Description("Estado")]
    public CarWashStatus? Status { get; set; }
    [Description("Precio total")]
    public decimal Price { get; set; }
    [Description("Comision operarios")]
    public decimal CommissionTotal { get; set; }
    [Description("Recargo fin de semana")]
    public decimal WeekendSurcharge { get; set; }
    [Description("Numero de cola")]
    public int QueueNumber { get; set; }
    [Description("Entrega estimada")]
    public DateTime? EstimatedDelivery { get; set; }
    [Description("Hora inicio")]
    public DateTime? StartTime { get; set; }
    [Description("Hora fin")]
    public DateTime? EndTime { get; set; }
    [Description("Pagado")]
    public bool IsPaid { get; set; }
    [Description("Metodo de pago")]
    public PaymentMethods? PaymentMethod { get; set; }
    [Description("Notas")]
    public string? Notes { get; set; }
    [Description("Zona")]
    public int? ZoneId { get; set; }
    [Description("Zona")]
    public ZoneDto? Zone { get; set; }
    [Description("Miembro")]
    public int? MemberId { get; set; }
    [Description("Miembro")]
    public MemberDto? Member { get; set; }
    [Description("Cobrar a mensualidad")]
    public bool ChargeToMonthly { get; set; }
    [Description("Tiene mensualidad")]
    public bool HasMonthlyMembership { get; set; }
    [Description("Mensualista")]
    public int? MonthlyMemberId { get; set; }
    [Description("Mensualista")]
    public string? MonthlyMemberName { get; set; }
    [Description("Adicionales")]
    public List<CarWashAdditionalDto> Additionals { get; set; } = new();
    [Description("Operarios")]
    public List<CarWashOperatorDto> Operators { get; set; } = new();
    [Description("Empresa")]
    public string? TenantId { get; set; }
    [Description("Empresa")]
    public TenantDto? Tenant { get; set; }
    [Description("Creado")]
    public DateTime? CreatedAt { get; set; }
    [Description("Creado por")]
    public string? CreatedBy { get; set; }
    [Description("Modificado")]
    public DateTime? LastModifiedAt { get; set; }
    [Description("Modificado por")]
    public string? LastModifiedBy { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CarWash, CarWashDto>(MemberList.None)
                .ForMember(x => x.Zone, y => y.Ignore())
                .ForMember(x => x.Tenant, y => y.Ignore())
                .ForMember(x => x.Member, y => y.Ignore())
                .ForMember(x => x.CreatedBy, y => y.Ignore())
                .ForMember(x => x.LastModifiedBy, y => y.Ignore())
                .ForMember(x => x.Additionals, y => y.Ignore())
                .ForMember(x => x.Operators, y => y.Ignore());
            CreateMap<CarWashDto, CarWash>(MemberList.None)
                .ForMember(x => x.Zone, y => y.Ignore())
                .ForMember(x => x.Tenant, y => y.Ignore())
                .ForMember(x => x.Member, y => y.Ignore())
                .ForMember(x => x.Additionals, y => y.Ignore())
                .ForMember(x => x.Operators, y => y.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedById, opt => opt.Ignore())
                .ForMember(dest => dest.DomainEvents, opt => opt.Ignore());
        }
    }
}

public class CarWashAdditionalDto
{
    public int Id { get; set; }
    public string AdditionalName { get; set; } = string.Empty;
    public decimal Price { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CarWashAdditional, CarWashAdditionalDto>(MemberList.None);
            CreateMap<CarWashAdditionalDto, CarWashAdditional>(MemberList.None);
        }
    }
}

public class CarWashOperatorDto
{
    public int Id { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public decimal Commission { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CarWashOperator, CarWashOperatorDto>(MemberList.None);
            CreateMap<CarWashOperatorDto, CarWashOperator>(MemberList.None);
        }
    }
}
