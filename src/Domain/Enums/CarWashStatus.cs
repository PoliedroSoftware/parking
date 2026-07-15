using System.ComponentModel;

namespace CleanArchitecture.Blazor.Domain.Enums;

public enum CarWashStatus
{
    [Description("En Cola")]
    Pending = 0,

    [Description("Lavando")]
    InProgress = 1,

    [Description("Lavado")]
    Completed = 2,

    [Description("Entregado")]
    Cancelled = 3
}
