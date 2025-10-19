using CleanArchitecture.Blazor.Application.Common.Interfaces.Identity;
using CleanArchitecture.Blazor.Application.Features.Charges.DTOs;
using CleanArchitecture.Blazor.Application.Features.Charges.Services;
using CleanArchitecture.Blazor.Application.Features.Identity.DTOs;
using CleanArchitecture.Blazor.Application.Features.Members.Service;
using CleanArchitecture.Blazor.Application.Features.Zones.DTOs;


namespace CleanArchitecture.Blazor.Server.UI.Components.Autocompletes;
#nullable disable warnings
public class SpaceGroupAutocomplete<T> : MudAutocomplete<SpaceGroupDto>
{
    public SpaceGroupAutocomplete()
    {
        SearchFunc = SearchKeyValues;
        ToStringFunc = dto => dto?.Name;
        Clearable = true;
        Dense = true;
        ResetValueOnEmptyText = true;
        ShowProgressIndicator = true;
        MaxItems = 200;
    }
    [Parameter] public string? TenantId { get; set; }
 

    [Inject] private ISpaceGroupService SpaceGroupService { get; set; } = default!;

    private Task<IEnumerable<SpaceGroupDto>> SearchKeyValues(string? value, CancellationToken cancellation)
    {
        var result = SpaceGroupService.DataSource.Where(x=>x.ZoneId>0);
        if (!string.IsNullOrWhiteSpace(value))
            result = SpaceGroupService.DataSource.Where(x => x.Name.Contains(value, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(result);
    }
    protected override void OnInitialized()
    {
        SpaceGroupService.OnChange += SpaceGroupService_OnChange;
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await SpaceGroupService.InitializeAsync();
        }

    }
    private async Task SpaceGroupService_OnChange()
    {
        await InvokeAsync(StateHasChanged);
    }
    protected override async ValueTask DisposeAsyncCore()
    {
        SpaceGroupService.OnChange -= SpaceGroupService_OnChange;
        await base.DisposeAsyncCore();
    }
}
