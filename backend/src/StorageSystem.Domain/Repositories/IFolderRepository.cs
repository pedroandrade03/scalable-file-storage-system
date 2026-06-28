using StorageSystem.Domain.Entities;

namespace StorageSystem.Domain.Repositories;

public interface IFolderRepository
{
    Task<Folder?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ExistsByNameAsync(
        Guid userId,
        Guid? parentFolderId,
        string name,
        CancellationToken cancellationToken
    );

    Task InsertAsync(Folder folder, CancellationToken cancellationToken);
}
