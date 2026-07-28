using Parking.Maui.Views;

namespace Parking.Maui;

public partial class App : Application
{
    private readonly LoginPage _loginPage;

    public App(LoginPage loginPage)
    {
        InitializeComponent();
        _loginPage = loginPage;
    }

    protected override Window CreateWindow(IActivationState? activationState) =>
        new(new NavigationPage(_loginPage));
}
