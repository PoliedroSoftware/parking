using CleanArchitecture.Blazor.Application.Features.VehicleTypeConfigurations.Commands.Create;
using CleanArchitecture.Blazor.Application.Features.VehicleTypeConfigurations.Commands.Delete;
using CleanArchitecture.Blazor.Application.Features.VehicleTypeConfigurations.Commands.Update;
using CleanArchitecture.Blazor.Application.Features.VehicleTypeConfigurations.Queries.GetActive;
using CleanArchitecture.Blazor.Application.Features.VehicleTypeConfigurations.Queries.GetAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Parking.Api.Controllers;

[ApiController, Route("api/v1/vehicle-types"), Authorize]
public sealed class VehicleTypesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetAllVehicleTypesQuery(), cancellationToken));

    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetActiveVehicleTypesQuery(), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VehicleTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateVehicleTypeCommand(request.Name, request.Icon, request.IsActive), cancellationToken);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { errors = result.Errors });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] VehicleTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateVehicleTypeCommand(id, request.Name, request.Icon, request.IsActive), cancellationToken);
        return result.Succeeded ? Ok(result.Data) : NotFound(new { errors = result.Errors });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteVehicleTypeCommand(id), cancellationToken);
        return result.Succeeded ? NoContent() : NotFound(new { errors = result.Errors });
    }

    public sealed record VehicleTypeRequest(string Name, string? Icon, bool IsActive = true);
}
