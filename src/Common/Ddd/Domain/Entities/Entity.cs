namespace Common.Ddd.Domain.Entities;

public abstract class Entity<TKey> : IEntity<TKey>
{
    public TKey Id { get; }

    protected Entity(TKey id)
    {
        Id = id;
    }
}