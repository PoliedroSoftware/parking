using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Application.Features.Carparks.Caching;
using CleanArchitecture.Blazor.Application.Features.Charges.Caching;
using CleanArchitecture.Blazor.Application.Features.Zones.DTOs;
using ZiggyCreatures.Caching.Fusion;

namespace CleanArchitecture.Blazor.Application.Features.Members.Service;

public class VehicleService: IVehicleService
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;
    private readonly IFusionCache _fusionCache;

    public VehicleService(
        IMapper mapper,
        IFusionCache fusionCache,
        IApplicationDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
        _fusionCache = fusionCache;
    }

    public event Func<Task>? OnChange;
    public List<VehicleDto> DataSource { get; private set; } = new();


    public async Task InitializeAsync()
    {
        await using var db = await _dbContextFactory.CreateAsync();
        DataSource = _fusionCache.GetOrSet(CarparkCacheKey.GetAllCacheKey,
            _ => db.Vehicles.ProjectTo<VehicleDto>(_mapper.ConfigurationProvider)
                .OrderBy(x => x.Name)
                .ToList()) ?? new List<VehicleDto>();
    }

    public async Task RefreshAsync()
    {
        _fusionCache.Remove(CarparkCacheKey.GetAllCacheKey);
        await using var db = await _dbContextFactory.CreateAsync();
        DataSource = _fusionCache.GetOrSet(ChargeCacheKey.GetAllCacheKey,
            _ => db.Vehicles.ProjectTo<VehicleDto>(_mapper.ConfigurationProvider)
                .OrderBy(x => x.Name)
                .ToList()) ?? new List<VehicleDto>();
        if (OnChange != null) await OnChange.Invoke();
    }
}
