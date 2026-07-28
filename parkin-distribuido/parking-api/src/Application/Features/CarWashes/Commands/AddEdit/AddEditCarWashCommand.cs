#nullable enable
#nullable disable warnings

using CleanArchitecture.Blazor.Application.Features.CarWashes.Caching;
using CleanArchitecture.Blazor.Application.Features.CarWashes.DTOs;
using CleanArchitecture.Blazor.Application.Features.Members.DTOs;
using CleanArchitecture.Blazor.Application.Features.Tenants.DTOs;
using CleanArchitecture.Blazor.Application.Features.Zones.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.CarWashes.Commands.AddEdit;

public class AddEditCarWashCommand: ICacheInvalidatorRequest<Result<int>>
{
    public int Id { get; set; }
    public string? LicensePlate {get;set;} 
    public VehicleTypes? VehicleType {get;set;} 
    public WashServiceType? WashServiceType {get;set;} 
    public CarWashStatus? Status {get;set;} 
    public decimal Price {get;set;}
    public decimal CommissionTotal {get;set;}
    public decimal WeekendSurcharge {get;set;}
    public int QueueNumber {get;set;}
    public DateTime? EstimatedDelivery {get;set;}
    public DateTime? StartTime {get;set;} 
    public DateTime? EndTime {get;set;} 
    public bool IsPaid {get;set;} 
    public PaymentMethods? PaymentMethod {get;set;} 
    public string? Notes {get;set;} 
    public int? ZoneId {get;set;} 
    public ZoneDto? Zone {get;set;}
    public int? MemberId {get;set;}
    public MemberDto? Member {get;set;}
    public bool ChargeToMonthly {get;set;}
    public List<CarWashAdditionalDto> Additionals {get;set;} = new();
    public List<CarWashOperatorDto> Operators {get;set;} = new();
    public string? TenantId {get;set;} 
    public TenantDto? Tenant {get;set;} 

    public string CacheKey => CarWashCacheKey.GetAllCacheKey;
    public IEnumerable<string>? Tags => CarWashCacheKey.Tags;
    
    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CarWashDto, AddEditCarWashCommand>(MemberList.None);
            CreateMap<AddEditCarWashCommand, CarWash>(MemberList.None)
                .ForMember(x => x.Additionals, y => y.Ignore())
                .ForMember(x => x.Operators, y => y.Ignore())
                .ForMember(x => x.Member, y => y.Ignore());
        }
    }
}

public class AddEditCarWashCommandHandler : IRequestHandler<AddEditCarWashCommand, Result<int>>
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContextFactory _dbContextFactory;
    
    public AddEditCarWashCommandHandler(IMapper mapper, IApplicationDbContextFactory dbContextFactory)
    {
        _mapper = mapper;
        _dbContextFactory = dbContextFactory;
    }
    
    public async Task<Result<int>> Handle(AddEditCarWashCommand request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
        if (request.Id > 0)
        {
            var item = await db.CarWashes.FindAsync(request.Id, cancellationToken);
            if (item == null)
                return await Result<int>.FailureAsync($"Lavado con id: [{request.Id}] no encontrado.");
            
            item = _mapper.Map(request, item);
            item.MemberId = ResolveMonthlyMemberId(request);
            await SyncAdditionals(db, item, request.Additionals, cancellationToken);
            await SyncOperators(db, item, request.Operators, cancellationToken);
            item.AddDomainEvent(new CarWashUpdatedEvent(item));
            await db.SaveChangesAsync(cancellationToken);
            return await Result<int>.SuccessAsync(item.Id);
        }
        else
        {
            var item = _mapper.Map<CarWash>(request);
            item.StartTime = request.StartTime ?? DateTime.Now;
            item.MemberId = ResolveMonthlyMemberId(request);
            item.Additionals = request.Additionals.Select(a => new CarWashAdditional { AdditionalName = a.AdditionalName, Price = a.Price }).ToList();
            item.Operators = request.Operators.Select(o => new CarWashOperator { OperatorName = o.OperatorName, Commission = o.Commission }).ToList();

            // Auto-assign queue number if not provided
            if (item.QueueNumber <= 0)
            {
                var startDate = item.StartTime!.Value;
                var todayMaxQ = await db.CarWashes
                    .Where(c => c.StartTime.HasValue && c.StartTime.Value.Year == startDate.Year && c.StartTime.Value.Month == startDate.Month && c.StartTime.Value.Day == startDate.Day)
                    .MaxAsync(c => (int?)c.QueueNumber, cancellationToken) ?? 0;
                item.QueueNumber = todayMaxQ + 1;
            }

            item.AddDomainEvent(new CarWashCreatedEvent(item));
            db.CarWashes.Add(item);
            await db.SaveChangesAsync(cancellationToken);
            return await Result<int>.SuccessAsync(item.Id);
        }
    }

    private static async Task SyncAdditionals(IApplicationDbContext db, CarWash item, List<CarWashAdditionalDto> dtos, CancellationToken ct)
    {
        var existing = await db.CarWashAdditionals.Where(x => x.CarWashId == item.Id).ToListAsync(ct);
        var toRemove = existing.Where(e => !dtos.Any(d => d.AdditionalName == e.AdditionalName)).ToList();
        if (toRemove.Any()) db.CarWashAdditionals.RemoveRange(toRemove);
        foreach (var dto in dtos)
        {
            if (!existing.Any(e => e.AdditionalName == dto.AdditionalName))
                db.CarWashAdditionals.Add(new CarWashAdditional { CarWashId = item.Id, AdditionalName = dto.AdditionalName, Price = dto.Price });
        }
    }

    private static int? ResolveMonthlyMemberId(AddEditCarWashCommand request)
    {
        return request.ChargeToMonthly
            ? request.Member?.Id ?? request.MemberId
            : null;
    }

    private static async Task SyncOperators(IApplicationDbContext db, CarWash item, List<CarWashOperatorDto> dtos, CancellationToken ct)
    {
        var existing = await db.CarWashOperators.Where(x => x.CarWashId == item.Id).ToListAsync(ct);
        var toRemove = existing.Where(e => !dtos.Any(d => d.OperatorName == e.OperatorName)).ToList();
        if (toRemove.Any()) db.CarWashOperators.RemoveRange(toRemove);
        foreach (var dto in dtos)
        {
            if (!existing.Any(e => e.OperatorName == dto.OperatorName))
                db.CarWashOperators.Add(new CarWashOperator { CarWashId = item.Id, OperatorName = dto.OperatorName, Commission = dto.Commission });
        }
    }
}
