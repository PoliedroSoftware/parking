using Parking.Shared.Services;

namespace Parking.Maui.Views;

public partial class MainPage : TabbedPage
{
    public MainPage(ParkingApiClient api)
    {
        InitializeComponent();
        foreach (var child in Children)
        {
            if (child is ParkingPage pp) pp.Initialize(api);
            else if (child is CarWashesPage cp) cp.Initialize(api);
            else if (child is MembersPage mp) mp.Initialize(api);
            else if (child is ReportsPage rp) rp.Initialize(api);
        }
    }
}
