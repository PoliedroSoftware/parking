using Parking.Shared.Services;

namespace Parking.Maui.Views;

public partial class CarWashesPage : ContentPage
{
    private ParkingApiClient? _api;
    public CarWashesPage() => InitializeComponent();
    public void Initialize(ParkingApiClient api) { _api = api; _ = Refresh(); }
    private async Task Refresh() { if (_api is null) return; var r = await _api.GetCarWashesAsync(); ItemsList.ItemsSource = r?.Items; }
    private async void OnRefresh(object? s, EventArgs e) => await Refresh();
    private async void OnPay(object? s, EventArgs e) { if (s is Button b && b.CommandParameter is CarWashItem item && _api != null) { await _api.PayCarWashAsync(item.Id, 1); await Refresh(); } }
}
