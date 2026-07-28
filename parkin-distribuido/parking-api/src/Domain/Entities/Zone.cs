using CleanArchitecture.Blazor.Domain.Common.Entities;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class Zone : BaseAuditableEntity
{
    public MultiCodeName Name { get; set; }
    public bool IsMain { get; set; }
    public int CarparkId { get; set; }
    public virtual Carpark Carpark { get; set; }

    public string HolidaySets { get; set; } = "1,0,0,0,0,0,1,1";

    public bool IsOpenCashbox { get; set; } = true;

    public virtual ICollection<Vehicle> Vehicles { get; set; } = [];

    public virtual ICollection<SpaceGroup> SpaceGroups { get; set; } = [];

    public virtual ICollection<Gate> Gates { get; set; } = [];

    public string? Description { get; set; }

    public int Capacity { get; set; } = 100;
    public int Adjustment { get; set; }
    public bool ManualFull { get; set; }
    public int GracePeriod { get; set; } = 15;
    public int ExitBuffer { get; set; } = 15;
    public decimal? LostTicketFee { get; set; } = 300;
    public decimal MonthlyDeposit { get; set; } = 150m;

}
