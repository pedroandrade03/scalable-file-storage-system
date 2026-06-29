using StorageSystem.Application.Exceptions;
using StorageSystem.Application.Interfaces;
using StorageSystem.Application.UseCases.Files.CreateFile;
using StorageSystem.Domain.Entities;
using StorageSystem.Domain.Repositories;

namespace StorageSystem.Application.Tests.UseCases.Files.CreateFile;

public class CreateFileCommandHandlerTests
{
    [Fact]
    public async Task Handle_CreatesFileMetadataAndUploadUrl_WhenCommandIsValid()
    {
        var userId = Guid.CreateVersion7();
        var folder = new Folder("Documents", userId);
        var fileRepository = new InMemoryFileRepository();
        var folderRepository = new InMemoryFolderRepository(folder);
        var userRepository = new InMemoryUserRepository(userId);
        var uploadUrlProvider = new InMemoryFileUploadUrlProvider();
        var unitOfWork = new InMemoryUnitOfWork();
        var handler = new CreateFileCommandHandler(
            fileRepository,
            folderRepository,
            userRepository,
            uploadUrlProvider,
            unitOfWork
        );

        var output = await handler.Handle(
            new CreateFileCommand("photo.png", "image/png", 123_456, folder.Id, userId),
            CancellationToken.None
        );

        Assert.NotEqual(Guid.Empty, output.Id);
        Assert.Equal("photo.png", output.Name);
        Assert.Equal("image/png", output.ContentType);
        Assert.Equal(123_456, output.SizeBytes);
        Assert.Equal(folder.Id, output.FolderId);
        Assert.Equal(userId, output.UserId);
        Assert.Contains(userId.ToString(), output.StorageKey);
        Assert.Contains(folder.Id.ToString(), output.StorageKey);
        Assert.Contains(output.Id.ToString(), output.StorageKey);
        Assert.Equal($"https://uploads.test/{output.StorageKey}", output.UploadUrl);
        Assert.Single(fileRepository.InsertedFiles);
        Assert.Equal(output.StorageKey, uploadUrlProvider.RequestedStorageKey);
        Assert.Equal("image/png", uploadUrlProvider.RequestedContentType);
        Assert.Equal(123_456, uploadUrlProvider.RequestedSizeBytes);
        Assert.True(unitOfWork.Committed);
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenUserDoesNotExist()
    {
        var folder = new Folder("Documents", Guid.CreateVersion7());
        var handler = new CreateHandler(
            fileRepository: new InMemoryFileRepository(),
            folderRepository: new InMemoryFolderRepository(folder),
            userRepository: new InMemoryUserRepository(),
            uploadUrlProvider: new InMemoryFileUploadUrlProvider(),
            unitOfWork: new InMemoryUnitOfWork()
        );

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new CreateFileCommand("photo.png", "image/png", 123_456, folder.Id, Guid.CreateVersion7()),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenFolderDoesNotExist()
    {
        var userId = Guid.CreateVersion7();
        var handler = new CreateHandler(
            fileRepository: new InMemoryFileRepository(),
            folderRepository: new InMemoryFolderRepository(),
            userRepository: new InMemoryUserRepository(userId),
            uploadUrlProvider: new InMemoryFileUploadUrlProvider(),
            unitOfWork: new InMemoryUnitOfWork()
        );

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new CreateFileCommand("photo.png", "image/png", 123_456, Guid.CreateVersion7(), userId),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Handle_ThrowsApplicationValidationException_WhenFolderBelongsToAnotherUser()
    {
        var userId = Guid.CreateVersion7();
        var folder = new Folder("Documents", Guid.CreateVersion7());
        var handler = new CreateHandler(
            fileRepository: new InMemoryFileRepository(),
            folderRepository: new InMemoryFolderRepository(folder),
            userRepository: new InMemoryUserRepository(userId),
            uploadUrlProvider: new InMemoryFileUploadUrlProvider(),
            unitOfWork: new InMemoryUnitOfWork()
        );

        await Assert.ThrowsAsync<ApplicationValidationException>(() =>
            handler.Handle(
                new CreateFileCommand("photo.png", "image/png", 123_456, folder.Id, userId),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Handle_ThrowsConflictException_WhenFileNameAlreadyExistsInFolder()
    {
        var userId = Guid.CreateVersion7();
        var folder = new Folder("Documents", userId);
        var fileRepository = new InMemoryFileRepository
        {
            ExistingFileName = "photo.png"
        };
        var unitOfWork = new InMemoryUnitOfWork();
        var handler = new CreateHandler(
            fileRepository,
            new InMemoryFolderRepository(folder),
            new InMemoryUserRepository(userId),
            new InMemoryFileUploadUrlProvider(),
            unitOfWork
        );

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(
                new CreateFileCommand("photo.png", "image/png", 123_456, folder.Id, userId),
                CancellationToken.None
            )
        );

        Assert.Empty(fileRepository.InsertedFiles);
        Assert.False(unitOfWork.Committed);
    }

    [Fact]
    public async Task Validator_RejectsInvalidMetadata()
    {
        var validator = new CreateFileCommandValidator();

        var result = await validator.ValidateAsync(
            new CreateFileCommand(" ", " ", 0, Guid.Empty, Guid.Empty)
        );

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateFileCommand.Name));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateFileCommand.ContentType));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateFileCommand.SizeBytes));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateFileCommand.FolderId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateFileCommand.UserId));
    }

    private sealed class CreateHandler(
        IFileRepository fileRepository,
        IFolderRepository folderRepository,
        IUserRepository userRepository,
        IFileUploadUrlProvider uploadUrlProvider,
        IUnitOfWork unitOfWork
    ) : CreateFileCommandHandler(
        fileRepository,
        folderRepository,
        userRepository,
        uploadUrlProvider,
        unitOfWork
    );

    private sealed class InMemoryFileRepository : IFileRepository
    {
        public string? ExistingFileName { get; init; }

        public List<FileItem> InsertedFiles { get; } = [];

        public Task<FileItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult<FileItem?>(null);

        public Task<bool> ExistsByNameAsync(
            Guid userId,
            Guid folderId,
            string name,
            CancellationToken cancellationToken
        )
        {
            var exists = string.Equals(ExistingFileName, name, StringComparison.OrdinalIgnoreCase);
            return Task.FromResult(exists);
        }

        public Task InsertAsync(FileItem file, CancellationToken cancellationToken)
        {
            InsertedFiles.Add(file);
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryFolderRepository(params Folder[] folders) : IFolderRepository
    {
        private readonly List<Folder> _folders = folders.ToList();

        public Task<Folder?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(_folders.SingleOrDefault(folder => folder.Id == id));

        public Task<bool> ExistsByNameAsync(
            Guid userId,
            Guid? parentFolderId,
            string name,
            CancellationToken cancellationToken
        )
            => Task.FromResult(false);

        public Task InsertAsync(Folder folder, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private sealed class InMemoryUserRepository(params Guid[] existingUserIds) : IUserRepository
    {
        private readonly HashSet<Guid> _existingUserIds = existingUserIds.ToHashSet();

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(_existingUserIds.Contains(id));
    }

    private sealed class InMemoryFileUploadUrlProvider : IFileUploadUrlProvider
    {
        public string? RequestedStorageKey { get; private set; }
        public string? RequestedContentType { get; private set; }
        public long? RequestedSizeBytes { get; private set; }

        public Task<string> CreateUploadUrlAsync(
            string storageKey,
            string contentType,
            long sizeBytes,
            CancellationToken cancellationToken
        )
        {
            RequestedStorageKey = storageKey;
            RequestedContentType = contentType;
            RequestedSizeBytes = sizeBytes;
            return Task.FromResult($"https://uploads.test/{storageKey}");
        }
    }

    private sealed class InMemoryUnitOfWork : IUnitOfWork
    {
        public bool Committed { get; private set; }

        public Task CommitAsync(CancellationToken cancellationToken)
        {
            Committed = true;
            return Task.CompletedTask;
        }
    }
}
