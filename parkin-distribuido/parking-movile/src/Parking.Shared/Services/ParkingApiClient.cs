using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Parking.Shared.Services;

public class ParkingApiClient
{
    private const string ApiPrefix = "/api/v1";
    private readonly HttpClient _http;
    private string? _token;

    public ParkingApiClient(HttpClient http)
    {
        _http = http;
    }

    public void SetToken(string token)
    {
        _token = token;
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    // Auth
    public async Task<LoginResult?> LoginAsync(string username, string password)
    {
        var response = await _http.PostAsJsonAsync($"{ApiPrefix}/auth/login", new { username, password });
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        if (result?.Token != null) SetToken(result.Token);
        return result;
    }

    // Parking
    public async Task<EntryResult?> CreateEntryAsync(string plate, string vehicleType)
    {
        var response = await _http.PostAsJsonAsync($"{ApiPrefix}/parking/entry", new { plate, vehicleType });
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<EntryResult>() : null;
    }

    public async Task<ExitResult?> ProcessExitAsync(string plate, string? customerName = null)
    {
        var response = await _http.PostAsJsonAsync($"{ApiPrefix}/parking/exit", new { plate, customerName });
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ExitResult>() : null;
    }

    public async Task<List<ActiveParking>?> GetActiveParkingAsync()
    {
        return await _http.GetFromJsonAsync<List<ActiveParking>>($"{ApiPrefix}/parking/active");
    }

    public async Task<List<TodayMovement>?> GetTodayMovementsAsync()
    {
        return await _http.GetFromJsonAsync<List<TodayMovement>>($"{ApiPrefix}/parking/today");
    }

    // CarWashes
    public async Task<PaginatedResult<CarWashItem>?> GetCarWashesAsync(int page = 1, int pageSize = 20)
    {
        return await _http.GetFromJsonAsync<PaginatedResult<CarWashItem>>($"{ApiPrefix}/carwashes?page={page}&pageSize={pageSize}");
    }

    public async Task<int?> CreateCarWashAsync(CreateCarWashRequest request)
    {
        var response = await _http.PostAsJsonAsync($"{ApiPrefix}/carwashes", request);
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<IdResult>();
        return result?.Id;
    }

    public async Task<bool> PayCarWashAsync(int id, int paymentMethod)
    {
        var response = await _http.PutAsJsonAsync($"{ApiPrefix}/carwashes/{id}/pay", new { paymentMethod });
        return response.IsSuccessStatusCode;
    }

    // Members
    public async Task<PaginatedResult<MemberItem>?> GetMembersAsync(int page = 1, string? search = null)
    {
        return await _http.GetFromJsonAsync<PaginatedResult<MemberItem>>($"{ApiPrefix}/members?page={page}&pageSize=20&search={search}");
    }

    public async Task<List<RentalItem>?> GetMemberRentalsAsync(int memberId)
    {
        return await _http.GetFromJsonAsync<List<RentalItem>>($"{ApiPrefix}/members/{memberId}/rentals");
    }

    public async Task<int?> PayMemberAsync(int memberId, int totalAmount, int paymentMethod)
    {
        var response = await _http.PostAsJsonAsync($"{ApiPrefix}/members/{memberId}/pay",
            new { totalAmount, paymentMethod });
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<PayResult>();
        return result?.RentalId;
    }

    public async Task<TicketResult?> GetMemberTicketAsync(int memberId, int? rentalId = null)
    {
        var url = $"{ApiPrefix}/members/{memberId}/print";
        if (rentalId.HasValue) url += $"?rentalId={rentalId.Value}";
        return await _http.GetFromJsonAsync<TicketResult>(url);
    }

    // Vehicle Types
    public async Task<List<VehicleTypeItem>?> GetActiveVehicleTypesAsync()
    {
        return await _http.GetFromJsonAsync<List<VehicleTypeItem>>($"{ApiPrefix}/vehicle-types/active");
    }

    // Reports
    public async Task<ArqueoResult?> GetArqueoAsync()
    {
        return await _http.GetFromJsonAsync<ArqueoResult>($"{ApiPrefix}/reports/arqueo");
    }

    public async Task<EstatusResult?> GetEstatusAsync()
    {
        return await _http.GetFromJsonAsync<EstatusResult>($"{ApiPrefix}/reports/estatus");
    }
}

// DTOs
public record LoginResult(string Token, string DisplayName, string Email, string[] Roles);
public record IdResult(int Id);
public record PayResult(int RentalId, bool Success);
public record TicketResult(string TicketText, string TicketHtml);
public record EntryResult(int RecordId, string TicketNumber, DateTime EntryTime, string TicketText, string TicketHtml);
public record ExitResult(int Amount, string Duration, string TicketText, string TicketHtml, DateTime ExitTime, DateTime EntryTime);
public record ActiveParking(string Plate, DateTime EntryTime, string VehicleType, int EstimatedAmount, string TicketNumber);
public record TodayMovement(int RecordId, string Plate, DateTime EntryTime, DateTime? ExitTime, string Status, int Amount, string VehicleType, string? CustomerName, string FormattedDuration, string? TicketNumber);
public record CarWashItem(int Id, string LicensePlate, string WashServiceType, string Status, decimal Price, bool IsPaid, string PaymentMethod, int QueueNumber, DateTime? StartTime, DateTime? EndTime, string? Notes);
public record CreateCarWashRequest(string LicensePlate, int VehicleType, int WashServiceType, decimal Price, string? Notes = null, int QueueNumber = 0);
public record MemberItem(int Id, string Name, string? LicensePlate, string? CardId, string? PhoneNumber, DateTime? StartDate, DateTime? ExpiryDate, bool IsActive, string? Notes);
public record RentalItem(int Id, DateTime? StartDate, DateTime? ExpiryDate, decimal RentalFee, decimal Deposit, decimal AmountDue, decimal AmountPaid, DateTime PaymentTime, string PaymentMethod, string? LicensePlate, string? CardId, string? Notes, string PaidMonth);
public record VehicleTypeItem(int Id, string Name, string? Icon);
public record ArqueoResult(int TotalEntradas, int TotalSalidas, int TotalLavados, int TotalMensualidades, int TotalParqueo, int TotalLavado, int TotalMensualidad, int TotalIngresos, List<ArqueoMovement> Movements);
public record ArqueoMovement(DateTime Time, string Type, string Placa, string Cliente, int Valor);
public record EstatusResult(List<ActiveParking> Parkings, List<WashStatus> Washes, int PagosDia, int TotalParkings, int TotalWashes, int LavadosEntregados, int LavadosPendientes);
public record WashStatus(string LicensePlate, string Servicio, string Estado, decimal Price, bool IsPaid);
public record PaginatedResult<T>(List<T> Items, int Total, int Page, int PageSize);
