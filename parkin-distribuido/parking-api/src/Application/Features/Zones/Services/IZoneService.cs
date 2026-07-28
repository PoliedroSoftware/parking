using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Application.Features.Zones.DTOs;
using CleanArchitecture.Blazor.Application.Features.Zones.Caching;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;

namespace CleanArchitecture.Blazor.Application.Features.Zones.Services;

public interface IZoneService
{
    List<ZoneDto> DataSource { get; }
    event Func<Task>? OnChange;
    Task InitializeAsync();
    Task RefreshAsync();
}

public class ZoneService : IZoneService
{
    private readonly IMapper _mapper;
    private readonly IFusionCache _fusionCache;
    private readonly IApplicationDbContextFactory _dbContextFactory;

    public event Func<Task>? OnChange;
    public List<ZoneDto> DataSource { get; private set; } = new();

    public ZoneService(
    IMapper mapper,
    IFusionCache fusionCache,
    IApplicationDbContextFactory dbContextFactory)
    {
        _mapper = mapper;
        _fusionCache = fusionCache;
        _dbContextFactory = dbContextFactory;
    }

    public async Task InitializeAsync() => DataSource = await LoadAsync(forceRefresh: false);

    public async Task RefreshAsync()
    {
        DataSource = await LoadAsync(forceRefresh: true);
        await RaiseOnChangeAsync();
    }

    private async Task<List<ZoneDto>> LoadAsync(bool forceRefresh)
    {
        if (forceRefresh)
        {
            _fusionCache.Remove(ZoneCacheKey.GetAllCacheKey);
        }
        await using var db = await _dbContextFactory.CreateAsync();
        var list = await _fusionCache.GetOrSetAsync(
        ZoneCacheKey.GetAllCacheKey,
        async _ => await QueryAsync(db)
        );
        return list ?? new List<ZoneDto>();
    }

    private async Task<List<ZoneDto>> QueryAsync(IApplicationDbContext db)
    {
        var zones = await db.Zones
        .AsNoTracking()
        .OrderBy(z => z.Name.Tc)
        .ToListAsync();
        
        return _mapper.Map<List<ZoneDto>>(zones);
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
