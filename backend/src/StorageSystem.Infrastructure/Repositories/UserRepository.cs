using Microsoft.EntityFrameworkCore;
using StorageSystem.Domain.Repositories;
using StorageSystem.Infrastructure.Persistence;

namespace StorageSystem.Infrastructure.Repositories;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        => context.Users.AnyAsync(user => user.Id == id, cancellationToken);
}
