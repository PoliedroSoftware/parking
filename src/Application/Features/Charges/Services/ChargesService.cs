using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Application.Features.Charges.Caching;
using CleanArchitecture.Blazor.Application.Features.Charges.DTOs;
using ZiggyCreatures.Caching.Fusion;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Blazor.Application.Features.Charges.Services;

public class ChargesService : IChargesService
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

    public async Task InitializeAsync() => DataSource = await LoadAsync(forceRefresh: false);

    public async Task RefreshAsync()
    {
        DataSource = await LoadAsync(forceRefresh: true);
        await RaiseOnChangeAsync();
    }

    private async Task<List<ChargeDto>> LoadAsync(bool forceRefresh)
    {
        if (forceRefresh)
        {
            _fusionCache.Remove(ChargeCacheKey.GetAllCacheKey);
        }

        await using var db = await _dbContextFactory.CreateAsync();
        var list = await _fusionCache.GetOrSetAsync(
            ChargeCacheKey.GetAllCacheKey,
            async _ => await QueryChargesAsync(db)
        );

        return list ?? new List<ChargeDto>();
    }

    private Task<List<ChargeDto>> QueryChargesAsync(IApplicationDbContext db)
    {
        var today = DateTime.Today;
        return db.Charges
            .Where(x => x.EffectiveDate > today)
            .ProjectTo<ChargeDto>(_mapper.ConfigurationProvider)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    private async Task RaiseOnChangeAsync()
    {
        var handler = OnChange;
        if (handler != null)
        {
            await handler.Invoke();
        }
    }
}
