using Parking.Shared.Services;

namespace Parking.Maui.Views;

public partial class MembersPage : ContentPage
{
    private ParkingApiClient? _api;
    public MembersPage() => InitializeComponent();
    public void Initialize(ParkingApiClient api) { _api = api; _ = Refresh(); }
    private async Task Refresh() { if (_api is null) return; var r = await _api.GetMembersAsync(); ItemsList.ItemsSource = r?.Items; }
    private async void OnRefresh(object? s, EventArgs e) => await Refresh();
    private async void OnPay(object? s, EventArgs e) { if (s is Button b && b.CommandParameter is MemberItem item && _api != null) { await _api.PayMemberAsync(item.Id, 120000, 1); await Refresh(); } }
    private async void OnHistory(object? s, EventArgs e) { if (s is Button b && b.CommandParameter is MemberItem item && _api != null) { var rentals = await _api.GetMemberRentalsAsync(item.Id); } }
}
