using StorageSystem.Application.Exceptions;
using StorageSystem.Application.Interfaces;
using StorageSystem.Application.UseCases.Files.GetFileDownload;
using StorageSystem.Domain.Entities;
using StorageSystem.Domain.Repositories;

namespace StorageSystem.Application.Tests.UseCases.Files.GetFileDownload;

public class GetFileDownloadQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsDownloadUrl_WhenFileExists()
    {
        var file = new FileItem(
            "example.txt",
            "text/plain",
            1024,
            Guid.CreateVersion7(),
            Guid.CreateVersion7()
        );
        var fileRepository = new InMemoryFileRepository(file);
        var downloadUrlProvider = new InMemoryFileDownloadUrlProvider();
        var handler = new GetFileDownloadQueryHandler(fileRepository, downloadUrlProvider);

        var output = await handler.Handle(
            new GetFileDownloadQuery(file.Id),
            CancellationToken.None
        );

        Assert.Equal(file.Id, output.FileId);
        Assert.Equal($"https://downloads.test/{file.StorageKey}", output.DownloadUrl);
        Assert.Equal(file.StorageKey, downloadUrlProvider.RequestedStorageKey);
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenFileDoesNotExist()
    {
        var handler = new GetFileDownloadQueryHandler(
            new InMemoryFileRepository(),
            new InMemoryFileDownloadUrlProvider()
        );

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new GetFileDownloadQuery(Guid.CreateVersion7()),
                CancellationToken.None
            )
        );
    }

    private sealed class InMemoryFileRepository(params FileItem[] files) : IFileRepository
    {
        private readonly List<FileItem> _files = files.ToList();

        public Task<FileItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(_files.SingleOrDefault(file => file.Id == id));

        public Task<bool> ExistsByNameAsync(
            Guid userId,
            Guid folderId,
            string name,
            CancellationToken cancellationToken
        )
            => Task.FromResult(false);

        public Task InsertAsync(FileItem file, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private sealed class InMemoryFileDownloadUrlProvider : IFileDownloadUrlProvider
    {
        public string? RequestedStorageKey { get; private set; }

        public Task<string> CreateDownloadUrlAsync(
            string storageKey,
            CancellationToken cancellationToken
        )
        {
            RequestedStorageKey = storageKey;
            return Task.FromResult($"https://downloads.test/{storageKey}");
        }
    }
}
