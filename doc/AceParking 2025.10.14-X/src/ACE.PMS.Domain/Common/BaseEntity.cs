
namespace ACE.PMS.Domain.Common;
public abstract class BaseEntity : IEntity<int>
{
    public virtual int Id { get; set; }
}