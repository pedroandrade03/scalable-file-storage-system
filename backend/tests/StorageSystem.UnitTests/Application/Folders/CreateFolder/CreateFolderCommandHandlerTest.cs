using FluentAssertions;
using Moq;
using StorageSystem.Application.Exceptions;
using StorageSystem.Application.UseCases.Folders.CreateFolder;
using DomainEntity = StorageSystem.Domain.Entities;

namespace StorageSystem.UnitTests.Application.Folders.CreateFolder;

[Collection(nameof(CreateFolderTestFixture))]
public class CreateFolderCommandHandlerTest
{
    private readonly CreateFolderTestFixture _fixture;

    public CreateFolderCommandHandlerTest(CreateFolderTestFixture fixture) => _fixture = fixture;

    [Fact(DisplayName = nameof(CreateRootFolder))]
    [Trait("Use Cases", "CreateFolder - Handler")]
    public async Task CreateRootFolder()
    {
        var folderRepository = _fixture.GetFolderRepositoryMock();
        var userRepository = _fixture.GetUserRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var userId = _fixture.GetValidUserId();
        var command = _fixture.GetValidCommand(userId);

        userRepository
            .Setup(r => r.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        folderRepository
            .Setup(r => r.ExistsByNameAsync(userId, null, command.Name.Trim(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new CreateFolderCommandHandler(
            folderRepository.Object,
            userRepository.Object,
            unitOfWork.Object
        );

        var output = await handler.Handle(command, CancellationToken.None);

        folderRepository.Verify(
            r => r.InsertAsync(It.IsAny<DomainEntity.Folder>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);

        output.Should().NotBeNull();
        output.Name.Should().Be(command.Name.Trim());
        output.UserId.Should().Be(userId);
        output.ParentFolderId.Should().BeNull();
        output.Id.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = nameof(CreateSubFolder))]
    [Trait("Use Cases", "CreateFolder - Handler")]
    public async Task CreateSubFolder()
    {
        var folderRepository = _fixture.GetFolderRepositoryMock();
        var userRepository = _fixture.GetUserRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var userId = _fixture.GetValidUserId();
        var parentFolder = _fixture.GetExampleFolder(userId);
        var command = _fixture.GetValidCommand(userId, parentFolder.Id);

        userRepository
            .Setup(r => r.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        folderRepository
            .Setup(r => r.GetByIdAsync(parentFolder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentFolder);

        folderRepository
            .Setup(r => r.ExistsByNameAsync(
                userId,
                parentFolder.Id,
                command.Name.Trim(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(false);

        var handler = new CreateFolderCommandHandler(
            folderRepository.Object,
            userRepository.Object,
            unitOfWork.Object
        );

        var output = await handler.Handle(command, CancellationToken.None);

        output.ParentFolderId.Should().Be(parentFolder.Id);
        folderRepository.Verify(
            r => r.GetByIdAsync(parentFolder.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact(DisplayName = nameof(ThrowWhenUserNotFound))]
    [Trait("Use Cases", "CreateFolder - Handler")]
    public async Task ThrowWhenUserNotFound()
    {
        var userRepository = _fixture.GetUserRepositoryMock();
        var command = _fixture.GetValidCommand();

        userRepository
            .Setup(r => r.ExistsAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new CreateFolderCommandHandler(
            _fixture.GetFolderRepositoryMock().Object,
            userRepository.Object,
            _fixture.GetUnitOfWorkMock().Object
        );

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"User '{command.UserId}' was not found.");
    }

    [Fact(DisplayName = nameof(ThrowWhenParentFolderNotFound))]
    [Trait("Use Cases", "CreateFolder - Handler")]
    public async Task ThrowWhenParentFolderNotFound()
    {
        var folderRepository = _fixture.GetFolderRepositoryMock();
        var userRepository = _fixture.GetUserRepositoryMock();
        var userId = _fixture.GetValidUserId();
        var parentFolderId = _fixture.GetValidUserId();
        var command = _fixture.GetValidCommand(userId, parentFolderId);

        userRepository
            .Setup(r => r.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        folderRepository
            .Setup(r => r.GetByIdAsync(parentFolderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainEntity.Folder?)null);

        var handler = new CreateFolderCommandHandler(
            folderRepository.Object,
            userRepository.Object,
            _fixture.GetUnitOfWorkMock().Object
        );

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Parent folder '{parentFolderId}' was not found.");
    }

    [Fact(DisplayName = nameof(ThrowWhenParentFolderBelongsToAnotherUser))]
    [Trait("Use Cases", "CreateFolder - Handler")]
    public async Task ThrowWhenParentFolderBelongsToAnotherUser()
    {
        var folderRepository = _fixture.GetFolderRepositoryMock();
        var userRepository = _fixture.GetUserRepositoryMock();
        var userId = _fixture.GetValidUserId();
        var parentFolder = _fixture.GetExampleFolder(_fixture.GetValidUserId());
        var command = _fixture.GetValidCommand(userId, parentFolder.Id);

        userRepository
            .Setup(r => r.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        folderRepository
            .Setup(r => r.GetByIdAsync(parentFolder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentFolder);

        var handler = new CreateFolderCommandHandler(
            folderRepository.Object,
            userRepository.Object,
            _fixture.GetUnitOfWorkMock().Object
        );

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<ApplicationValidationException>()
            .WithMessage("Parent folder must belong to the same user.");
    }

    [Fact(DisplayName = nameof(ThrowWhenFolderNameAlreadyExists))]
    [Trait("Use Cases", "CreateFolder - Handler")]
    public async Task ThrowWhenFolderNameAlreadyExists()
    {
        var folderRepository = _fixture.GetFolderRepositoryMock();
        var userRepository = _fixture.GetUserRepositoryMock();
        var userId = _fixture.GetValidUserId();
        var command = _fixture.GetValidCommand(userId);

        userRepository
            .Setup(r => r.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        folderRepository
            .Setup(r => r.ExistsByNameAsync(userId, null, command.Name.Trim(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateFolderCommandHandler(
            folderRepository.Object,
            userRepository.Object,
            _fixture.GetUnitOfWorkMock().Object
        );

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage($"Folder '{command.Name.Trim()}' already exists in this location.");
    }
}
