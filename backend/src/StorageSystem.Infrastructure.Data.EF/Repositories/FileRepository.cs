using Microsoft.EntityFrameworkCore;
using StorageSystem.Domain.Entities;
using StorageSystem.Domain.Repositories;
using StorageSystem.Infrastructure.Data.EF.Persistence.Contexts;

namespace StorageSystem.Infrastructure.Data.EF.Repositories;

public class FileRepository(ApplicationDbContext context) : IFileRepository
{
    private readonly DbSet<FileItem> _files = context.Files;

    public Task<FileItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _files.SingleOrDefaultAsync(file => file.Id == id, cancellationToken);

    public Task<FileItem?> GetByIdAndUserIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken
    )
        => _files.SingleOrDefaultAsync(
            file => file.Id == id && file.UserId == userId,
            cancellationToken
        );

    public Task<bool> ExistsByNameAsync(
        Guid userId,
        Guid folderId,
        string name,
        CancellationToken cancellationToken
    )
        => _files.AnyAsync(
            file =>
                file.UserId == userId &&
                file.FolderId == folderId &&
                file.Name.ToLower() == name.ToLower(),
            cancellationToken
        );

    public Task<bool> ExistsInFolderAsync(
        Guid folderId,
        Guid userId,
        CancellationToken cancellationToken
    )
        => _files.AnyAsync(
            file =>
                file.FolderId == folderId &&
                file.UserId == userId,
            cancellationToken
        );

    public async Task<IReadOnlyList<FileItem>> ListByFolderAsync(
        Guid userId,
        Guid folderId,
        CancellationToken cancellationToken
    )
        => await _files
            .Where(file =>
                file.UserId == userId &&
                file.FolderId == folderId
            )
            .OrderBy(file => file.Name)
            .ToListAsync(cancellationToken);

    public async Task InsertAsync(FileItem file, CancellationToken cancellationToken)
        => await _files.AddAsync(file, cancellationToken);

    public Task DeleteAsync(FileItem file, CancellationToken cancellationToken)
    {
        _files.Remove(file);
        return Task.CompletedTask;
    }
}
