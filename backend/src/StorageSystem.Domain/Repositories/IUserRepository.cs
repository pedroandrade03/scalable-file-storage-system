using StorageSystem.Domain.Entities;

namespace StorageSystem.Domain.Repositories;

public interface IUserRepository
{
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User?> GetByExternalIdentityAsync(
        string externalProvider,
        string externalSubject,
        CancellationToken cancellationToken
    );
    Task InsertAsync(User user, CancellationToken cancellationToken);
}
