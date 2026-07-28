using CleanArchitecture.Blazor.Application.Features.VehicleTypeConfigurations.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.VehicleTypeConfigurations.Queries.GetActive;

public sealed record GetActiveVehicleTypesQuery : IRequest<IReadOnlyList<VehicleTypeDto>>;

public sealed class GetActiveVehicleTypesQueryHandler(IApplicationDbContextFactory dbFactory)
    : IRequestHandler<GetActiveVehicleTypesQuery, IReadOnlyList<VehicleTypeDto>>
{
    public async Task<IReadOnlyList<VehicleTypeDto>> Handle(GetActiveVehicleTypesQuery request, CancellationToken cancellationToken)
    {
        await using var db = await dbFactory.CreateAsync(cancellationToken);
        return await db.VehicleTypeConfigs.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new VehicleTypeDto(x.Id, x.Name, x.Icon, x.IsActive))
            .ToListAsync(cancellationToken);
    }
}
