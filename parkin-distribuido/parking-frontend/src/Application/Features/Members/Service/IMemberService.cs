using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Application.Features.Members.DTOs;
using CleanArchitecture.Blazor.Application.Features.Members.Caching;
using CleanArchitecture.Blazor.Application.Features.Members.DTOs;
using ZiggyCreatures.Caching.Fusion;

namespace CleanArchitecture.Blazor.Application.Features.Members.Service;

public interface IMemberService
{
    List<MemberDto> DataSource { get; }
    event Func<Task>? OnChange;
    Task InitializeAsync();
    Task RefreshAsync();
}

public class MemberService : IMemberService
{
    private readonly IMapper _mapper;
    private readonly IFusionCache _fusionCache;
    private readonly IApplicationDbContextFactory _dbContextFactory;

    public event Func<Task>? OnChange;
    public List<MemberDto> DataSource { get; private set; } = new();

    public MemberService(
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

    private async Task<List<MemberDto>> LoadAsync(bool forceRefresh)
    {
        if (forceRefresh)
        {
            _fusionCache.Remove(MemberCacheKey.GetAllCacheKey);
        }
        await using var db = await _dbContextFactory.CreateAsync();
        var list = await _fusionCache.GetOrSetAsync(
        MemberCacheKey.GetAllCacheKey,
        async _ => await QueryAsync(db)
        );
        return list ?? new List<MemberDto>();
    }

    private Task<List<MemberDto>> QueryAsync(IApplicationDbContext db)
    {
        return db.Members
        .AsNoTracking()
        .OrderBy(z => z.Name)
        .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
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
