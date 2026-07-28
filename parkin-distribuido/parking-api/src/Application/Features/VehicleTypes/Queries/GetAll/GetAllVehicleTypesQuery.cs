using CleanArchitecture.Blazor.Application.Features.VehicleTypeConfigurations.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.VehicleTypeConfigurations.Queries.GetAll;

public sealed record GetAllVehicleTypesQuery : IRequest<IReadOnlyList<VehicleTypeDto>>;

public sealed class GetAllVehicleTypesQueryHandler(IApplicationDbContextFactory dbFactory)
    : IRequestHandler<GetAllVehicleTypesQuery, IReadOnlyList<VehicleTypeDto>>
{
    public async Task<IReadOnlyList<VehicleTypeDto>> Handle(GetAllVehicleTypesQuery request, CancellationToken cancellationToken)
    {
        await using var db = await dbFactory.CreateAsync(cancellationToken);
        return await db.VehicleTypeConfigs.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new VehicleTypeDto(x.Id, x.Name, x.Icon, x.IsActive))
            .ToListAsync(cancellationToken);
    }
}
