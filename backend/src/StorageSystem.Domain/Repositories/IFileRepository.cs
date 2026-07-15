using StorageSystem.Domain.Entities;

namespace StorageSystem.Domain.Repositories;

public interface IFileRepository
{
    Task<FileItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<FileItem?> GetByIdAndUserIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken
    );

    Task<bool> ExistsByNameAsync(
        Guid userId,
        Guid folderId,
        string name,
        CancellationToken cancellationToken
    );

    Task<bool> ExistsInFolderAsync(
        Guid folderId,
        Guid userId,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<FileItem>> ListByFolderAsync(
        Guid userId,
        Guid folderId,
        CancellationToken cancellationToken
    );

    Task InsertAsync(FileItem file, CancellationToken cancellationToken);

    Task DeleteAsync(FileItem file, CancellationToken cancellationToken);
}
