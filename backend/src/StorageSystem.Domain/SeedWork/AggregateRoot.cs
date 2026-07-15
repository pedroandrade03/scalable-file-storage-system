namespace StorageSystem.Domain.SeedWork;

public abstract class AggregateRoot : AuditableEntity, IAggregateRoot
{
    protected AggregateRoot()
    {
    }
}
