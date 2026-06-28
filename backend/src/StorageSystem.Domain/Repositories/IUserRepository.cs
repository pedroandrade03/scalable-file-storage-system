namespace StorageSystem.Domain.Repositories;

public interface IUserRepository
{
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
}
