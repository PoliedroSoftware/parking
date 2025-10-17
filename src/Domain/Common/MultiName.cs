using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Blazor.Domain.Common;

public record MultiName(string? En, string? Tc);
public record MultiCodeName(string Code, string En, string Tc);
