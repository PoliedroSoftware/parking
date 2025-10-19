using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Application.Features.Zones.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.Members.Service;

internal interface IVehicleService
{
    List<VehicleDto> DataSource { get; }
    event Func<Task>? OnChange;
    Task InitializeAsync();
    Task RefreshAsync();
}
