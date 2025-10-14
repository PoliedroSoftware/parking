
namespace ACE.PMS.Domain.Entities;
public class SpaceGroup : BaseAuditableEntity
{
    public string Name { get; set; } // Monthly Group Name    
    public int Capacity { get; set; } = 1; // Number of currently assigned members    
    public required Zone Zone { get; set; }
    public string? Description { get; set; } // Description    
        
    public SpaceGroup(string name, int capacity)
    {
        Name = name;
        Capacity = capacity;
    }
}
