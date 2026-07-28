using Parking.Shared.Services;

namespace Parking.Maui.Views;

public partial class ReportsPage : ContentPage
{
    private ParkingApiClient? _api;
    public ReportsPage() => InitializeComponent();
    public void Initialize(ParkingApiClient api) { _api = api; }
    private async void OnArqueo(object? s, EventArgs e) { if (_api is null) return; var r = await _api.GetArqueoAsync(); if (r != null) ResultLabel.Text = $"Entradas:{r.TotalEntradas} Salidas:{r.TotalSalidas} Lavados:{r.TotalLavados} Mensual:{r.TotalMensualidades} TOTAL:$ {r.TotalIngresos:N0}"; }
    private async void OnEstatus(object? s, EventArgs e) { if (_api is null) return; var r = await _api.GetEstatusAsync(); if (r != null) ResultLabel.Text = $"Activos:{r.TotalParkings} Entregados:{r.LavadosEntregados} Pendientes:{r.LavadosPendientes} Pagos:$ {r.PagosDia:N0}"; }
}
