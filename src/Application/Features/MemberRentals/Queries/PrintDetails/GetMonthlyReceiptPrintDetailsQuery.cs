#nullable enable
#nullable disable warnings

using CleanArchitecture.Blazor.Application.Features.MemberRentals.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.MemberRentals.Queries.PrintDetails;

public class GetMonthlyReceiptPrintDetailsQuery : IRequest<Result<MonthlyReceiptPrintDetailsDto>>
{
    public int? MemberRentalId { get; set; }
    public int? MemberId { get; set; }
}

public class GetMonthlyReceiptPrintDetailsQueryHandler :
    IRequestHandler<GetMonthlyReceiptPrintDetailsQuery, Result<MonthlyReceiptPrintDetailsDto>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;

    public GetMonthlyReceiptPrintDetailsQueryHandler(IApplicationDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Result<MonthlyReceiptPrintDetailsDto>> Handle(
        GetMonthlyReceiptPrintDetailsQuery request,
        CancellationToken cancellationToken)
    {
        if (!request.MemberRentalId.HasValue && !request.MemberId.HasValue)
            return await Result<MonthlyReceiptPrintDetailsDto>.FailureAsync("Debe indicar la mensualidad a imprimir.");

        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);

        var rentalQuery = db.MemberRentals.AsNoTracking();
        rentalQuery = request.MemberRentalId.HasValue
            ? rentalQuery.Where(x => x.Id == request.MemberRentalId.Value)
            : rentalQuery
                .Where(x => x.MemberId == request.MemberId.Value)
                .OrderByDescending(x => x.PaymentTime)
                .ThenByDescending(x => x.Id);

        var rental = await rentalQuery
            .Select(x => new MonthlyReceiptPrintDetailsDto
            {
                MemberRentalId = x.Id,
                MemberId = x.MemberId,
                MemberName = x.Member != null ? x.Member.Name : null,
                LicensePlate = x.LicensePlate,
                CardId = x.CardId,
                StartDate = x.StartDate,
                ExpiryDate = x.ExpiryDate,
                AmountPaid = x.AmountPaid,
                PaymentTime = x.PaymentTime,
                PaymentMethodId = x.PaymentMethodId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (rental is null)
            return await Result<MonthlyReceiptPrintDetailsDto>.FailureAsync("No se encontro una mensualidad para imprimir.");

        rental.MemberId ??= request.MemberId;

        if (rental.MemberId.HasValue)
        {
            var member = await db.Members.AsNoTracking()
                .Where(x => x.Id == rental.MemberId.Value)
                .Select(x => new { x.Name, x.LicensePlate })
                .FirstOrDefaultAsync(cancellationToken);

            rental.MemberName ??= member?.Name;
            rental.LicensePlate = string.IsNullOrWhiteSpace(rental.LicensePlate)
                ? member?.LicensePlate
                : rental.LicensePlate;

            rental.Vehicles = await db.MemberVehicles.AsNoTracking()
                .Where(x => x.MemberId == rental.MemberId.Value && x.Vehicle != null)
                .Select(x => new MonthlyReceiptVehicleDto
                {
                    LicensePlate = x.Vehicle!.Name,
                    VehicleType = x.Vehicle.VehicleTypeId
                })
                .OrderBy(x => x.LicensePlate)
                .ToListAsync(cancellationToken);
        }

        if (rental.Vehicles.Count == 0 && !string.IsNullOrWhiteSpace(rental.LicensePlate))
        {
            rental.Vehicles.Add(new MonthlyReceiptVehicleDto
            {
                LicensePlate = rental.LicensePlate.Trim().ToUpperInvariant()
            });
        }

        var plates = rental.Vehicles
            .Select(x => x.LicensePlate.Trim().ToUpperInvariant())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        var pendingWashesQuery = db.CarWashes.AsNoTracking()
            .Where(x => x.ChargeToMonthly && x.Status != CarWashStatus.Cancelled);

        if (rental.MemberId.HasValue && plates.Count > 0)
        {
            var memberId = rental.MemberId.Value;
            pendingWashesQuery = pendingWashesQuery
                .Where(x => x.MemberId == memberId || plates.Contains(x.LicensePlate.ToUpper()));
        }
        else if (rental.MemberId.HasValue)
        {
            var memberId = rental.MemberId.Value;
            pendingWashesQuery = pendingWashesQuery.Where(x => x.MemberId == memberId);
        }
        else if (plates.Count > 0)
        {
            pendingWashesQuery = pendingWashesQuery.Where(x => plates.Contains(x.LicensePlate.ToUpper()));
        }

        rental.PendingWashes = await pendingWashesQuery
            .OrderBy(x => x.StartTime ?? x.CreatedAt)
            .Select(x => new MonthlyReceiptPendingWashDto
            {
                Id = x.Id,
                LicensePlate = x.LicensePlate,
                WashServiceType = x.WashServiceType,
                Status = x.Status,
                Price = x.Price
            })
            .ToListAsync(cancellationToken);

        return await Result<MonthlyReceiptPrintDetailsDto>.SuccessAsync(rental);
    }
}
