namespace StorageSystem.Domain.SeedWork;

public abstract class AuditableEntity
{
    public Guid Id { get; protected set; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.Now;
    public DateTimeOffset? UpdatedAt { get; protected set; }
}
