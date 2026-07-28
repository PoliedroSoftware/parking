using Parking.Shared.Services;

namespace Parking.Maui.Views;

public partial class LoginPage : ContentPage
{
    private readonly ParkingApiClient _api;

    public LoginPage(ParkingApiClient api)
    {
        InitializeComponent();
        _api = api;
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        var result = await _api.LoginAsync(UsernameEntry.Text, PasswordEntry.Text);
        if (result is null) { ErrorLabel.Text = "Usuario o contrasena invalidos"; ErrorLabel.IsVisible = true; return; }
        await Navigation.PushModalAsync(new MainPage(_api));
    }
}
