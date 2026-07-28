using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Blazor.Server.UI.Services.Decoupled;

public sealed class ParkingApiClient(HttpClient httpClient, IOptions<ParkingApiOptions> options)
{
    private const string ApiPrefix = "/api/v1";

    public async Task<ApiLoginResponse?> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            $"{ApiPrefix}/auth/login",
            new { username, password },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<ApiLoginResponse>(cancellationToken);
        if (!string.IsNullOrWhiteSpace(result?.Token))
            SetToken(result.Token);

        return result;
    }

    public async Task<ApiEntryResponse?> CreateEntryAsync(
        string plate,
        string vehicleType,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            $"{ApiPrefix}/parking/entry",
            new { plate, vehicleType },
            cancellationToken);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<ApiEntryResponse>(cancellationToken)
            : null;
    }

    public async Task<ApiExitResponse?> ProcessExitAsync(
        string plate,
        string? customerName = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            $"{ApiPrefix}/parking/exit",
            new { plate, customerName },
            cancellationToken);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<ApiExitResponse>(cancellationToken)
            : null;
    }

    public Task<List<ApiActiveParking>?> GetActiveAsync(CancellationToken cancellationToken = default) =>
        httpClient.GetFromJsonAsync<List<ApiActiveParking>>($"{ApiPrefix}/parking/active", cancellationToken);

    public Task<List<ApiTodayMovement>?> GetTodayAsync(CancellationToken cancellationToken = default) =>
        httpClient.GetFromJsonAsync<List<ApiTodayMovement>>($"{ApiPrefix}/parking/today", cancellationToken);

    private void SetToken(string token)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}

public sealed record ApiLoginResponse(string Token, string DisplayName, string Email, string[] Roles);

public sealed record ApiEntryResponse(
    int RecordId,
    string TicketNumber,
    DateTime EntryTime,
    string TicketText,
    string TicketHtml);

public sealed record ApiExitResponse(
    int Amount,
    string Duration,
    string TicketText,
    string TicketHtml,
    DateTime ExitTime,
    DateTime EntryTime);

public sealed record ApiActiveParking(
    string Plate,
    DateTime EntryTime,
    string VehicleType,
    int EstimatedAmount,
    string TicketNumber);

public sealed record ApiTodayMovement(
    int RecordId,
    string Plate,
    DateTime EntryTime,
    DateTime? ExitTime,
    string Status,
    int Amount,
    string VehicleType,
    string? CustomerName,
    string FormattedDuration,
    string? TicketNumber);
