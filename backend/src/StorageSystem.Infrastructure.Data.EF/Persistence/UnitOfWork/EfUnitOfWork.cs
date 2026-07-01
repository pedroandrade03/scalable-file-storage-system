using StorageSystem.Application.Interfaces;
using StorageSystem.Infrastructure.Data.EF.Persistence.Contexts;

namespace StorageSystem.Infrastructure.Data.EF.Persistence.UnitOfWork;

public class EfUnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    public Task CommitAsync(CancellationToken cancellationToken)
        => context.SaveChangesAsync(cancellationToken);

    public Task RollbackAsync(CancellationToken cancellationToken)
    {
        context.ChangeTracker.Clear();
        return Task.CompletedTask;
    }
}
