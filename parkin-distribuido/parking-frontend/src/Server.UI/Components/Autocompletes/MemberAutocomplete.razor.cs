using CleanArchitecture.Blazor.Application.Common.Interfaces.Identity;
using CleanArchitecture.Blazor.Application.Features.Charges.DTOs;
using CleanArchitecture.Blazor.Application.Features.Charges.Services;
using CleanArchitecture.Blazor.Application.Features.Identity.DTOs;
using CleanArchitecture.Blazor.Application.Features.Members.DTOs;
using CleanArchitecture.Blazor.Application.Features.Members.Service;


namespace CleanArchitecture.Blazor.Server.UI.Components.Autocompletes;
#nullable disable warnings
public class MemberAutocomplete<T> : MudAutocomplete<MemberDto>
{
    public MemberAutocomplete()
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


    [Inject] private IMemberService MemberService { get; set; } = default!;

    private Task<IEnumerable<MemberDto>> SearchKeyValues(string? value, CancellationToken cancellation)
    {
        var result = MemberService.DataSource.Where(x =>
            (x.TenantId != null && x.TenantId.Equals(TenantId) || TenantId == null));
        if (!string.IsNullOrWhiteSpace(value))
            result = MemberService.DataSource.Where(x => (x.TenantId != null && x.TenantId.Equals(TenantId) || TenantId == null) &&
                                                       (x.Name.Contains(value,
                                                            StringComparison.OrdinalIgnoreCase) ||
                                                            x.PhoneNumber.Contains(value,
                                                            StringComparison.OrdinalIgnoreCase) ||
                                                        x.Email.Contains(value, StringComparison.OrdinalIgnoreCase)));
        return Task.FromResult(result);
    }
    protected override void OnInitialized()
    {
        MemberService.OnChange += MemberService_OnChange;
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await MemberService.InitializeAsync();
        }

    }
    private async Task MemberService_OnChange()
    {
        await InvokeAsync(StateHasChanged);
    }
    protected override async ValueTask DisposeAsyncCore()
    {
        MemberService.OnChange -= MemberService_OnChange;
        await base.DisposeAsyncCore();
    }
}
