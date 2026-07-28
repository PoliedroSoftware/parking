using System.Reflection;
using CleanArchitecture.Blazor.Application.Features.VehicleTypeConfigurations.Commands.Create;
using CleanArchitecture.Blazor.Application.Features.VehicleTypeConfigurations.Queries.GetAll;
using MediatR;
using Parking.Api.Controllers;

namespace Parking.Api.UnitTests;

public sealed class ArchitectureTests
{
    [Fact]
    public void Domain_DoesNotDependOnOuterApplicationLayers()
    {
        var references = typeof(CleanArchitecture.Blazor.Domain.Entities.VehicleTypeConfig)
            .Assembly.GetReferencedAssemblies().Select(x => x.Name).ToHashSet();

        Assert.DoesNotContain("CleanArchitecture.Blazor.Application", references);
        Assert.DoesNotContain("CleanArchitecture.Blazor.Infrastructure", references);
        Assert.DoesNotContain("Parking.Api", references);
    }

    [Fact]
    public void Application_ContainsMediatRRequestsAndHandlers()
    {
        var applicationAssembly = typeof(CreateVehicleTypeCommand).Assembly;
        var handlers = applicationAssembly.GetTypes()
            .Count(type => type.GetInterfaces().Any(@interface =>
                @interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)));

        Assert.True(handlers > 0);
        Assert.Contains(typeof(GetAllVehicleTypesQuery), applicationAssembly.GetTypes());
    }

    [Fact]
    public void VehicleTypesController_UsesApplicationMediatorBoundary()
    {
        var constructor = typeof(VehicleTypesController).GetConstructors().Single();
        var parameters = constructor.GetParameters();

        Assert.Single(parameters);
        Assert.Equal(typeof(ISender), parameters[0].ParameterType);
    }
}
