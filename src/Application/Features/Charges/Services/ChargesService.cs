using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Application.Features.Charges.Caching;
using CleanArchitecture.Blazor.Application.Features.Charges.DTOs;
using CleanArchitecture.Blazor.Application.Features.Tenants.Caching;
using CleanArchitecture.Blazor.Application.Features.Tenants.DTOs;
using ZiggyCreatures.Caching.Fusion;

namespace CleanArchitecture.Blazor.Application.Features.Charges.Services;

public class ChargesService:IChargesService
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;
    private readonly IFusionCache _fusionCache;

    public ChargesService(
        IMapper mapper,
        IFusionCache fusionCache,
        IApplicationDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
        _fusionCache = fusionCache;
    }

    public event Func<Task>? OnChange;
    public List<ChargeDto> DataSource { get; private set; } = new();


    public async Task InitializeAsync()
    {
        await using var db = await _dbContextFactory.CreateAsync();
        DataSource = _fusionCache.GetOrSet(ChargeCacheKey.GetAllCacheKey,
            _ => db.Charges.Where(x=>x.EffectiveDate>DateTime.Today).ProjectTo<ChargeDto>(_mapper.ConfigurationProvider)
                .OrderBy(x => x.Name)
                .ToList()) ?? new List<ChargeDto>();
    }

    public async Task RefreshAsync()
    {
        _fusionCache.Remove(TenantCacheKey.TenantsCacheKey);
        await using var db = await _dbContextFactory.CreateAsync();
        DataSource = _fusionCache.GetOrSet(ChargeCacheKey.GetAllCacheKey,
            _ => db.Charges.Where(x => x.EffectiveDate > DateTime.Today).ProjectTo<ChargeDto>(_mapper.ConfigurationProvider)
                .OrderBy(x => x.Name)
                .ToList()) ?? new List<ChargeDto>();
        if (OnChange != null) await OnChange.Invoke();
    }
}
