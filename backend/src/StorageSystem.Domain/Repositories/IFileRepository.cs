using StorageSystem.Domain.Entities;

namespace StorageSystem.Domain.Repositories;

public interface IFileRepository
{
    Task<bool> ExistsByNameAsync(
        Guid userId,
        Guid folderId,
        string name,
        CancellationToken cancellationToken
    );

    Task InsertAsync(FileItem file, CancellationToken cancellationToken);
}
