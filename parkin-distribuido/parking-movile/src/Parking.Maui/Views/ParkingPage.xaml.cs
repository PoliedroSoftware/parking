using Parking.Shared.Services;

namespace Parking.Maui.Views;

public partial class ParkingPage : ContentPage
{
    private ParkingApiClient? _api;

    public ParkingPage() => InitializeComponent();

    public void Initialize(ParkingApiClient api) { _api = api; _ = LoadData(); }

    private async Task LoadData() { if (_api is null) return; var items = await _api.GetActiveParkingAsync(); ActiveList.ItemsSource = items; }

    private async void OnProcessClicked(object? sender, EventArgs e)
    {
        if (_api is null || string.IsNullOrWhiteSpace(PlateEntry.Text)) return;
        var plate = PlateEntry.Text.Trim().ToUpperInvariant(); PlateEntry.Text = "";
        var actives = await _api.GetActiveParkingAsync();
        var existing = actives?.FirstOrDefault(a => a.Plate == plate);
        if (existing != null) await DoExit(existing);
        else await DoEntry(plate);
        await LoadData();
    }

    private async Task DoEntry(string plate) { var vt = (await _api.GetActiveVehicleTypesAsync())?.FirstOrDefault()?.Name ?? "Carro"; var r = await _api.CreateEntryAsync(plate, vt); if (r != null) MessageLabel.Text = $"ENTRADA: {plate}"; }
    private async Task DoExit(ActiveParking p) { var r = await _api.ProcessExitAsync(p.Plate); if (r != null) MessageLabel.Text = $"SALIDA: {p.Plate} - $ {r.Amount:N0}"; }
    private async void OnExitClicked(object? sender, EventArgs e) { if (sender is Button b && b.CommandParameter is ActiveParking p) { await DoExit(p); await LoadData(); } }
}
