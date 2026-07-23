using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Models;

namespace CleanArchitecture.Blazor.Application.Features.ParkingPayments.Commands.Adjust;

public class AdjustParkingPaymentCommand : IRequest<Result<int>>
{
    public int ParkingRecordId { get; set; }
    public decimal NewAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class AdjustParkingPaymentCommandHandler(
    IApplicationDbContextFactory dbContextFactory)
    : IRequestHandler<AdjustParkingPaymentCommand, Result<int>>
{
    private const string ActiveParkingStatus = "Activo";

    public async Task<Result<int>> Handle(AdjustParkingPaymentCommand request, CancellationToken cancellationToken)
    {
        if (request.ParkingRecordId <= 0)
            return await Result<int>.FailureAsync("Debe indicar el parqueo a modificar.");

        if (request.NewAmount < 0)
            return await Result<int>.FailureAsync("El valor pagado no puede ser negativo.");

        var reason = request.Reason?.Trim();
        if (string.IsNullOrWhiteSpace(reason))
            return await Result<int>.FailureAsync("Debe indicar el motivo del ajuste.");

        await using var db = await dbContextFactory.CreateAsync(cancellationToken);
        var record = await db.ParkingRecords.FindAsync(request.ParkingRecordId, cancellationToken);
        if (record is null)
            return await Result<int>.FailureAsync("No se encontro el parqueo a modificar.");

        if (record.Status == ActiveParkingStatus || !record.ExitTime.HasValue)
            return await Result<int>.FailureAsync("Solo se puede modificar el pago de un parqueo finalizado.");

        var previousAmount = record.Amount;
        if (previousAmount == request.NewAmount)
            return await Result<int>.FailureAsync("El nuevo valor debe ser diferente al valor actual.");

        record.Amount = request.NewAmount;
        record.Notes = AppendAdjustmentNote(record.Notes, previousAmount, request.NewAmount, reason);

        await db.SaveChangesAsync(cancellationToken);
        return await Result<int>.SuccessAsync(record.Id);
    }

    private static string AppendAdjustmentNote(string? notes, decimal previousAmount, decimal newAmount, string reason)
    {
        var currentNotes = notes?.Trim();
        var adjustment = $"Ajuste pago {DateTime.Now:yyyy-MM-dd HH:mm}: $ {previousAmount:N0} -> $ {newAmount:N0}. {reason}";

        return string.IsNullOrWhiteSpace(currentNotes)
            ? adjustment
            : $"{currentNotes} | {adjustment}";
    }
}
