using Microsoft.Extensions.Logging;
using Parking.Maui.Views;
using Parking.Shared.Services;

namespace Parking.Maui;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddSingleton(sp =>
        {
            // Distributed API default; Preferences can override this for local development.
            var defaultApiUrl = "http://192.168.0.137:5221";
            var apiUrl = Preferences.Default.Get("parking_api_url", defaultApiUrl).TrimEnd('/');
            var http = new HttpClient { BaseAddress = new Uri(apiUrl) };
            return http;
        });
        builder.Services.AddSingleton<ParkingApiClient>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<ParkingPage>();
        builder.Services.AddTransient<CarWashesPage>();
        builder.Services.AddTransient<MembersPage>();
        builder.Services.AddTransient<ReportsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
