using System.ComponentModel;

namespace CleanArchitecture.Blazor.Domain.Enums;

public enum WashServiceType
{
    [Description("Ninguno")]
    None = 0,

    [Description("Basico")]
    Basic = 1,

    [Description("Premium")]
    Premium = 2,

    [Description("Full Detail")]
    FullDetail = 3,

    [Description("Solo Carroceria")]
    BodyOnly = 4,

    [Description("Tapiceria")]
    Upholstery = 5,
}
