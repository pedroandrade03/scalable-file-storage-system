using Microsoft.EntityFrameworkCore;
using StorageSystem.Domain.Entities;
using StorageSystem.Domain.Repositories;
using StorageSystem.Infrastructure.Persistence;

namespace StorageSystem.Infrastructure.Repositories;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        => context.Users.AnyAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        => context.Users.FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

    public Task<User?> GetByExternalIdentityAsync(
        string externalProvider,
        string externalSubject,
        CancellationToken cancellationToken
    ) => context.Users.FirstOrDefaultAsync(
        user =>
            user.ExternalProvider == externalProvider &&
            user.ExternalSubject == externalSubject,
        cancellationToken
    );

    public async Task InsertAsync(User user, CancellationToken cancellationToken)
        => await context.Users.AddAsync(user, cancellationToken);
}
