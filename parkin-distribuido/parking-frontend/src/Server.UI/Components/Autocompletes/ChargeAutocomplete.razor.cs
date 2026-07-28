using CleanArchitecture.Blazor.Application.Common.Interfaces.Identity;
using CleanArchitecture.Blazor.Application.Features.Charges.DTOs;
using CleanArchitecture.Blazor.Application.Features.Charges.Services;
using CleanArchitecture.Blazor.Application.Features.Identity.DTOs;


namespace CleanArchitecture.Blazor.Server.UI.Components.Autocompletes;
#nullable disable warnings
public class ChargeAutocomplete<T> : MudAutocomplete<ChargeDto>
{
    public ChargeAutocomplete()
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
 

    [Inject] private IChargesService ChargesService { get; set; } = default!;

    private Task<IEnumerable<ChargeDto>> SearchKeyValues(string? value, CancellationToken cancellation)
    {
        var result = ChargesService.DataSource.Where(x =>
            (x.TenantId != null && x.TenantId.Equals(TenantId) || TenantId==null));
        if (!string.IsNullOrWhiteSpace(value))
            result = ChargesService.DataSource.Where(x => (x.TenantId != null && x.TenantId.Equals(TenantId) || TenantId == null)  &&
                                                       (x.Name.Contains(value,
                                                            StringComparison.OrdinalIgnoreCase) ||
                                                        x.Name.Contains(value, StringComparison.OrdinalIgnoreCase)));
        return Task.FromResult(result);
    }
    protected override void OnInitialized()
    {
        ChargesService.OnChange += ChargesService_OnChange;
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await ChargesService.InitializeAsync();
        }

    }
    private async Task ChargesService_OnChange()
    {
        await InvokeAsync(StateHasChanged);
    }
    protected override async ValueTask DisposeAsyncCore()
    {
        ChargesService.OnChange -= ChargesService_OnChange;
        await base.DisposeAsyncCore();
    }
}
