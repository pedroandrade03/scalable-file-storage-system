using Microsoft.EntityFrameworkCore;
using StorageSystem.Domain.Entities;
using StorageSystem.Domain.Repositories;
using StorageSystem.Infrastructure.Data.EF.Persistence.Contexts;

namespace StorageSystem.Infrastructure.Data.EF.Repositories;

public class FolderRepository(ApplicationDbContext context) : IFolderRepository
{
    private readonly DbSet<Folder> _folders = context.Folders;

    public Task<Folder?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _folders.FirstOrDefaultAsync(folder => folder.Id == id, cancellationToken);

    public Task<Folder?> GetByIdAndUserIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken
    )
        => _folders.FirstOrDefaultAsync(
            folder => folder.Id == id && folder.UserId == userId,
            cancellationToken
        );

    public Task<bool> ExistsByNameAsync(
        Guid userId,
        Guid? parentFolderId,
        string name,
        CancellationToken cancellationToken
    )
        => _folders.AnyAsync(
            folder =>
                folder.UserId == userId &&
                folder.ParentFolderId == parentFolderId &&
                folder.Name.ToLower() == name.ToLower(),
            cancellationToken
        );

    public Task<bool> HasSubFoldersAsync(
        Guid folderId,
        Guid userId,
        CancellationToken cancellationToken
    )
        => _folders.AnyAsync(
            folder =>
                folder.ParentFolderId == folderId &&
                folder.UserId == userId,
            cancellationToken
        );

    public async Task InsertAsync(Folder folder, CancellationToken cancellationToken)
        => await _folders.AddAsync(folder, cancellationToken);

    public Task DeleteAsync(Folder folder, CancellationToken cancellationToken)
    {
        _folders.Remove(folder);
        return Task.CompletedTask;
    }
}
