using StorageSystem.Domain.Entities;

namespace StorageSystem.Domain.Repositories;

public interface IFolderRepository
{
    Task<Folder?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Folder?> GetByIdAndUserIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken
    );

    Task<bool> ExistsByNameAsync(
        Guid userId,
        Guid? parentFolderId,
        string name,
        CancellationToken cancellationToken
    );

    Task<bool> HasSubFoldersAsync(
        Guid folderId,
        Guid userId,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<Folder>> ListByParentAsync(
        Guid userId,
        Guid? parentFolderId,
        CancellationToken cancellationToken
    );

    Task InsertAsync(Folder folder, CancellationToken cancellationToken);

    Task DeleteAsync(Folder folder, CancellationToken cancellationToken);
}
