using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Application.Features.Charges.DTOs;
using CleanArchitecture.Blazor.Application.Features.Zones.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.Members.Service;

public interface ISpaceGroupService
{
    List<SpaceGroupDto> DataSource { get; }
    event Func<Task>? OnChange;
    Task InitializeAsync();
    Task RefreshAsync();
}
