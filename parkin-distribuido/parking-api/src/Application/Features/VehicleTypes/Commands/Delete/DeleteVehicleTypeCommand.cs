namespace CleanArchitecture.Blazor.Application.Features.VehicleTypeConfigurations.Commands.Delete;

public sealed record DeleteVehicleTypeCommand(int Id) : IRequest<Result>;

public sealed class DeleteVehicleTypeCommandHandler(IApplicationDbContextFactory dbFactory)
    : IRequestHandler<DeleteVehicleTypeCommand, Result>
{
    public async Task<Result> Handle(DeleteVehicleTypeCommand request, CancellationToken cancellationToken)
    {
        await using var db = await dbFactory.CreateAsync(cancellationToken);
        var entity = await db.VehicleTypeConfigs.FindAsync([request.Id], cancellationToken);
        if (entity is null)
            return await Result.FailureAsync($"Tipo de vehículo {request.Id} no existe.");

        db.VehicleTypeConfigs.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return await Result.SuccessAsync();
    }
}
