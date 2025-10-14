
namespace ACE.PMS.Domain.Entities;
public class Holiday : BaseAuditableEntity
{
    public DateTime Date { get; set; }
    public string Name_En { get; set; } = string.Empty;
    public string Name_Tc { get; set; } = string.Empty;
    
    public Holiday(DateTime date )
    {
        Date = date;
    }
}
