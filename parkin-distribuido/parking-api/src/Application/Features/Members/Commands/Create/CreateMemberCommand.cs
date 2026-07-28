#nullable enable
#nullable disable warnings

using CleanArchitecture.Blazor.Application.Features.Members.Caching;
using CleanArchitecture.Blazor.Application.Features.Members.DTOs;
using CleanArchitecture.Blazor.Application.Features.Vehicles.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.Members.Commands.Create;

public class CreateMemberCommand : ICacheInvalidatorRequest<Result<int>>
{
    [Description("Id")] public int Id { get; set; }
    [Description("License plate")] public string? LicensePlate { get; set; }
    [Description("Card id")] public string? CardId { get; set; }
    [Description("Start date")] public DateTime? StartDate { get; set; }
    [Description("Expiry date")] public DateTime? ExpiryDate { get; set; }
    [Description("Is active")] public bool IsActive { get; set; } = true;
    [Description("Name")] public string Name { get; set; } = string.Empty;
    [Description("Phone number")] public string? PhoneNumber { get; set; }
    [Description("Notes")] public string? Notes { get; set; }
    [Description("Member Vehicles")] public List<VehicleDto>? MemberVehicles { get; set; } = new();
    [Description("Amount")] public decimal? Amount { get; set; }
    [Description("Amount Note")] public string? AmountNote { get; set; }

    public string CacheKey => MemberCacheKey.GetAllCacheKey;
    public IEnumerable<string>? Tags => MemberCacheKey.Tags;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CreateMemberCommand, Member>(MemberList.None)
                .ForMember(x => x.SpaceGroup, y => y.Ignore())
                .ForMember(x => x.MemberRentals, y => y.Ignore())
                .ForMember(x => x.MemberVehicles, y => y.Ignore())
                .ForMember(x => x.Tenant, y => y.Ignore());
        }
    }
}

public class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, Result<int>>
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly ILogger<CreateMemberCommandHandler> _logger;

    public CreateMemberCommandHandler(IMapper mapper, IApplicationDbContextFactory dbContextFactory, ILogger<CreateMemberCommandHandler> logger)
    {
        _mapper = mapper;
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creando miembro: Nombre={Name}, Placa={Plate}, Vehiculos={Count}",
                request.Name, request.LicensePlate, request.MemberVehicles?.Count ?? 0);

            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
        
            request.StartDate ??= DateTime.Today;
            request.Name ??= string.Empty;
            _logger.LogInformation("StartDate={StartDate}, Amount={Amount}, Name={Name}", request.StartDate, request.Amount, request.Name);
        
            var item = _mapper.Map<Member>(request);
            _logger.LogInformation("Member mapeado. StartDate={D}, Name={N}", item.StartDate, item.Name);
            
        item.ExpiryDate = request.StartDate.Value.AddMonths(1);
        item.LicensePlate = !string.IsNullOrWhiteSpace(request.LicensePlate) ? request.LicensePlate 
            : request.MemberVehicles?.FirstOrDefault()?.Name ?? $"MEM-{Guid.NewGuid():N}"[..12];
        item.CardId = item.LicensePlate;
        item.MemberRentals = new List<MemberRental>();
            item.MemberVehicles = new List<MemberVehicle>();
            item.IsActive = true;

            var zone = await db.Zones.FirstOrDefaultAsync(z => z.IsMain, cancellationToken)
                       ?? await db.Zones.FirstOrDefaultAsync(cancellationToken);
            _logger.LogInformation("Zona encontrada: {ZoneId} {ZoneName}", zone?.Id, zone?.Name);

            foreach (var vDto in request.MemberVehicles ?? Enumerable.Empty<VehicleDto>())
            {
                _logger.LogInformation("Creando vehiculo: Name={Name}, Type={Type}", vDto.Name, vDto.VehicleTypeId);
                var vehicle = new Vehicle
                {
                    Name = vDto.Name,
                    VehicleTypeId = vDto.VehicleTypeId ?? VehicleTypes.PrivateCar,
                    ServiceCategoryId = ServiceCategories.Monthly,
                    Capacity = 1,
                    IsActive = true,
                    ZoneId = zone?.Id,
                    Zone = zone
                };
                db.Vehicles.Add(vehicle);
                item.MemberVehicles.Add(new MemberVehicle { Vehicle = vehicle });
            }

        var rentalFee = new MemberRental
        {
            LicensePlate = item.LicensePlate,
            CardId = item.CardId,
                StartDate = request.StartDate.Value,
                ExpiryDate = item.ExpiryDate,
                RentalFee = request.Amount ?? 0,
                AmountPaid = request.Amount ?? 0,
                PaymentTime = DateTime.Now,
                PaymentMethodId = PaymentMethods.Cash,
                Notes = request.AmountNote
            };
            item.MemberRentals.Add(rentalFee);

            item.AddDomainEvent(new MemberCreatedEvent(item));
            db.Members.Add(item);
            
            _logger.LogInformation("Guardando en BD...");
            await db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Miembro creado exitosamente. Id={Id}", item.Id);
            
            return await Result<int>.SuccessAsync(item.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR al crear miembro: {Message}", ex.Message);
            if (ex.InnerException != null)
                _logger.LogError("Inner: {Inner}", ex.InnerException.Message);
            return await Result<int>.FailureAsync($"Error: {ex.Message}");
        }
    }
}
