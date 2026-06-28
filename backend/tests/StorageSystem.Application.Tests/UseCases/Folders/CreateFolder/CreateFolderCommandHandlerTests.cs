using StorageSystem.Application.Exceptions;
using StorageSystem.Application.Interfaces;
using StorageSystem.Application.UseCases.Folders.CreateFolder;
using StorageSystem.Domain.Entities;
using StorageSystem.Domain.Repositories;

namespace StorageSystem.Application.Tests.UseCases.Folders.CreateFolder;

public class CreateFolderCommandHandlerTests
{
    [Fact]
    public async Task Handle_CreatesRootFolder_WhenCommandIsValid()
    {
        var userId = Guid.CreateVersion7();
        var folderRepository = new InMemoryFolderRepository();
        var userRepository = new InMemoryUserRepository(userId);
        var unitOfWork = new InMemoryUnitOfWork();
        var handler = new CreateFolderCommandHandler(folderRepository, userRepository, unitOfWork);

        var output = await handler.Handle(
            new CreateFolderCommand("Documents", userId, null),
            CancellationToken.None
        );

        Assert.NotEqual(Guid.Empty, output.Id);
        Assert.Equal("Documents", output.Name);
        Assert.Equal(userId, output.UserId);
        Assert.Null(output.ParentFolderId);
        Assert.Single(folderRepository.InsertedFolders);
        Assert.True(unitOfWork.Committed);
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenUserDoesNotExist()
    {
        var folderRepository = new InMemoryFolderRepository();
        var userRepository = new InMemoryUserRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var handler = new CreateFolderCommandHandler(folderRepository, userRepository, unitOfWork);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new CreateFolderCommand("Documents", Guid.CreateVersion7(), null),
                CancellationToken.None
            )
        );

        Assert.Empty(folderRepository.InsertedFolders);
        Assert.False(unitOfWork.Committed);
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenParentFolderDoesNotExist()
    {
        var userId = Guid.CreateVersion7();
        var folderRepository = new InMemoryFolderRepository();
        var userRepository = new InMemoryUserRepository(userId);
        var unitOfWork = new InMemoryUnitOfWork();
        var handler = new CreateFolderCommandHandler(folderRepository, userRepository, unitOfWork);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new CreateFolderCommand("Documents", userId, Guid.CreateVersion7()),
                CancellationToken.None
            )
        );

        Assert.Empty(folderRepository.InsertedFolders);
        Assert.False(unitOfWork.Committed);
    }

    [Fact]
    public async Task Handle_ThrowsApplicationValidationException_WhenParentFolderBelongsToAnotherUser()
    {
        var userId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();
        var parentFolder = new Folder("Parent", otherUserId);
        var folderRepository = new InMemoryFolderRepository(parentFolder);
        var userRepository = new InMemoryUserRepository(userId);
        var unitOfWork = new InMemoryUnitOfWork();
        var handler = new CreateFolderCommandHandler(folderRepository, userRepository, unitOfWork);

        await Assert.ThrowsAsync<ApplicationValidationException>(() =>
            handler.Handle(
                new CreateFolderCommand("Documents", userId, parentFolder.Id),
                CancellationToken.None
            )
        );

        Assert.Empty(folderRepository.InsertedFolders);
        Assert.False(unitOfWork.Committed);
    }

    [Fact]
    public async Task Handle_ThrowsConflictException_WhenFolderNameAlreadyExistsInSameParent()
    {
        var userId = Guid.CreateVersion7();
        var folderRepository = new InMemoryFolderRepository
        {
            ExistingFolderName = "Documents"
        };
        var userRepository = new InMemoryUserRepository(userId);
        var unitOfWork = new InMemoryUnitOfWork();
        var handler = new CreateFolderCommandHandler(folderRepository, userRepository, unitOfWork);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(
                new CreateFolderCommand("Documents", userId, null),
                CancellationToken.None
            )
        );

        Assert.Empty(folderRepository.InsertedFolders);
        Assert.False(unitOfWork.Committed);
    }

    [Fact]
    public async Task Validator_RejectsEmptyName()
    {
        var validator = new CreateFolderCommandValidator();

        var result = await validator.ValidateAsync(
            new CreateFolderCommand(" ", Guid.CreateVersion7(), null)
        );

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateFolderCommand.Name));
    }

    private sealed class InMemoryFolderRepository(params Folder[] folders) : IFolderRepository
    {
        private readonly List<Folder> _folders = folders.ToList();

        public string? ExistingFolderName { get; init; }

        public List<Folder> InsertedFolders { get; } = [];

        public Task<Folder?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(_folders.SingleOrDefault(folder => folder.Id == id));

        public Task<bool> ExistsByNameAsync(
            Guid userId,
            Guid? parentFolderId,
            string name,
            CancellationToken cancellationToken
        )
        {
            var exists = string.Equals(ExistingFolderName, name, StringComparison.OrdinalIgnoreCase);
            return Task.FromResult(exists);
        }

        public Task InsertAsync(Folder folder, CancellationToken cancellationToken)
        {
            InsertedFolders.Add(folder);
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryUserRepository(params Guid[] existingUserIds) : IUserRepository
    {
        private readonly HashSet<Guid> _existingUserIds = existingUserIds.ToHashSet();

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(_existingUserIds.Contains(id));
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
