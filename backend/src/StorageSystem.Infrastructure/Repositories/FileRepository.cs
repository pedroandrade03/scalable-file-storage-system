using Microsoft.EntityFrameworkCore;
using StorageSystem.Domain.Entities;
using StorageSystem.Domain.Repositories;
using StorageSystem.Infrastructure.Persistence;

namespace StorageSystem.Infrastructure.Repositories;

public class FileRepository(ApplicationDbContext context) : IFileRepository
{
    private readonly DbSet<FileItem> _files = context.Files;

    public Task<FileItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _files.SingleOrDefaultAsync(file => file.Id == id, cancellationToken);

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

    public async Task InsertAsync(FileItem file, CancellationToken cancellationToken)
        => await _files.AddAsync(file, cancellationToken);
}
