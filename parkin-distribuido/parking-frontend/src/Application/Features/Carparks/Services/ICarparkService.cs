using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Application.Features.Carparks.DTOs;
using CleanArchitecture.Blazor.Application.Features.Carparks.Caching;
using ZiggyCreatures.Caching.Fusion;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Blazor.Application.Features.Carparks.Services;

public interface ICarparkService
{
    List<CarparkDto> DataSource { get; }
    event Func<Task>? OnChange;
    Task InitializeAsync();
    Task RefreshAsync();
}

public class CarparkService : ICarparkService
{
    private readonly IMapper _mapper;
    private readonly IFusionCache _fusionCache;
    private readonly IApplicationDbContextFactory _dbContextFactory;

    public event Func<Task>? OnChange;
    public List<CarparkDto> DataSource { get; private set; } = new();

    public CarparkService(
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

    private async Task<List<CarparkDto>> LoadAsync(bool forceRefresh)
    {
        if (forceRefresh)
        {
            _fusionCache.Remove(CarparkCacheKey.GetAllCacheKey);
        }
        await using var db = await _dbContextFactory.CreateAsync();
        var list = await _fusionCache.GetOrSetAsync(
        CarparkCacheKey.GetAllCacheKey,
        async _ => await QueryAsync(db)
        );
        return list ?? new List<CarparkDto>();
    }

    private Task<List<CarparkDto>> QueryAsync(IApplicationDbContext db)
    {
        return db.Carparks
        .AsNoTracking()
        .OrderBy(c => c.Name.Tc) // 按代碼或名称排序，避免复杂对象排序异常
        .ProjectTo<CarparkDto>(_mapper.ConfigurationProvider)
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

