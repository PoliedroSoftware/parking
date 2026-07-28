namespace Parking.Api.DTOs;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, string DisplayName, string Email, string[] Roles);

public record EntryRequest(string Plate, string VehicleType);
public record EntryResponse(
    int RecordId,
    string TicketNumber,
    DateTime EntryTime,
    string TicketText,
    string TicketHtml);

public record ExitRequest(string Plate, string? CustomerName = null);
public record ExitResponse(
    int Amount,
    string Duration,
    string TicketText,
    string TicketHtml,
    DateTime ExitTime,
    DateTime EntryTime);

public record ActiveParkingDto(
    string Plate,
    DateTime EntryTime,
    string VehicleType,
    int EstimatedAmount,
    string TicketNumber);

public record TodayMovementDto(
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
