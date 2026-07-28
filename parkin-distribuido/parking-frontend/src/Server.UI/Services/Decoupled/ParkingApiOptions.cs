namespace CleanArchitecture.Blazor.Server.UI.Services.Decoupled;

public sealed class ParkingApiOptions
{
    public const string SectionName = "ParkingApi";

    public string BaseUrl { get; set; } = "http://localhost:5220";
}
