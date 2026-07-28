

namespace ACE.PMS.Domain.Common;
public interface IEntity
{
}

public interface IEntity<TKey> : IEntity
{
    TKey Id { get; set; }
}
