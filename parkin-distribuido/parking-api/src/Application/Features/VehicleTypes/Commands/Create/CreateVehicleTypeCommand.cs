using CleanArchitecture.Blazor.Application.Features.VehicleTypeConfigurations.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.VehicleTypeConfigurations.Commands.Create;

public sealed record CreateVehicleTypeCommand(string Name, string? Icon, bool IsActive = true) : IRequest<Result<VehicleTypeDto>>;

public sealed class CreateVehicleTypeCommandHandler(IApplicationDbContextFactory dbFactory)
    : IRequestHandler<CreateVehicleTypeCommand, Result<VehicleTypeDto>>
{
    public async Task<Result<VehicleTypeDto>> Handle(CreateVehicleTypeCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return await Result<VehicleTypeDto>.FailureAsync("El nombre es obligatorio.");

        await using var db = await dbFactory.CreateAsync(cancellationToken);
        var entity = new VehicleTypeConfig { Name = request.Name.Trim(), Icon = request.Icon, IsActive = request.IsActive };
        db.VehicleTypeConfigs.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return await Result<VehicleTypeDto>.SuccessAsync(new(entity.Id, entity.Name, entity.Icon, entity.IsActive));
    }
}
