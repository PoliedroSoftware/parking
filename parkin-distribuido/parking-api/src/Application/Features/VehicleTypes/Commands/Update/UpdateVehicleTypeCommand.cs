using CleanArchitecture.Blazor.Application.Features.VehicleTypeConfigurations.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.VehicleTypeConfigurations.Commands.Update;

public sealed record UpdateVehicleTypeCommand(int Id, string Name, string? Icon, bool IsActive) : IRequest<Result<VehicleTypeDto>>;

public sealed class UpdateVehicleTypeCommandHandler(IApplicationDbContextFactory dbFactory)
    : IRequestHandler<UpdateVehicleTypeCommand, Result<VehicleTypeDto>>
{
    public async Task<Result<VehicleTypeDto>> Handle(UpdateVehicleTypeCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return await Result<VehicleTypeDto>.FailureAsync("El nombre es obligatorio.");

        await using var db = await dbFactory.CreateAsync(cancellationToken);
        var entity = await db.VehicleTypeConfigs.FindAsync([request.Id], cancellationToken);
        if (entity is null)
            return await Result<VehicleTypeDto>.FailureAsync($"Tipo de vehículo {request.Id} no existe.");

        entity.Name = request.Name.Trim();
        entity.Icon = request.Icon;
        entity.IsActive = request.IsActive;
        await db.SaveChangesAsync(cancellationToken);
        return await Result<VehicleTypeDto>.SuccessAsync(new(entity.Id, entity.Name, entity.Icon, entity.IsActive));
    }
}
