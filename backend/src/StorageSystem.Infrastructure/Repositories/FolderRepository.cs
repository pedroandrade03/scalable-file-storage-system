using Microsoft.EntityFrameworkCore;
using StorageSystem.Domain.Entities;
using StorageSystem.Domain.Repositories;
using StorageSystem.Infrastructure.Persistence;

namespace StorageSystem.Infrastructure.Repositories;

public class FolderRepository(ApplicationDbContext context) : IFolderRepository
{
    private readonly DbSet<Folder> _folders = context.Folders;

    public Task<Folder?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _folders.FirstOrDefaultAsync(folder => folder.Id == id, cancellationToken);

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

    public async Task InsertAsync(Folder folder, CancellationToken cancellationToken)
        => await _folders.AddAsync(folder, cancellationToken);
}
