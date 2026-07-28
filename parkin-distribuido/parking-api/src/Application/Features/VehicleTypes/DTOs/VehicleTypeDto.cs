namespace CleanArchitecture.Blazor.Application.Features.VehicleTypeConfigurations.DTOs;

public sealed record VehicleTypeDto(int Id, string Name, string? Icon, bool IsActive);
