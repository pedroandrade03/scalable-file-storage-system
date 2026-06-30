using FluentAssertions;
using Moq;
using StorageSystem.Application.Exceptions;
using StorageSystem.Application.UseCases.Files.CreateFile;
using DomainEntity = StorageSystem.Domain.Entities;

namespace StorageSystem.UnitTests.Application.Files.CreateFile;

[Collection(nameof(CreateFileTestFixture))]
public class CreateFileCommandHandlerTest
{
    private readonly CreateFileTestFixture _fixture;

    public CreateFileCommandHandlerTest(CreateFileTestFixture fixture) => _fixture = fixture;

    [Fact(DisplayName = nameof(CreateFile))]
    [Trait("Use Cases", "CreateFile - Handler")]
    public async Task CreateFile()
    {
        var fileRepository = _fixture.GetFileRepositoryMock();
        var folderRepository = _fixture.GetFolderRepositoryMock();
        var userRepository = _fixture.GetUserRepositoryMock();
        var uploadUrlProvider = _fixture.GetUploadUrlProviderMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var userId = _fixture.GetValidUserId();
        var folder = _fixture.GetExampleFolder(userId);
        var command = _fixture.GetValidCommand(userId, folder.Id);
        const string uploadUrl = "https://minio.local/upload";

        userRepository
            .Setup(r => r.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        folderRepository
            .Setup(r => r.GetByIdAsync(folder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(folder);

        fileRepository
            .Setup(r => r.ExistsByNameAsync(
                userId,
                folder.Id,
                command.Name.Trim(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(false);

        uploadUrlProvider
            .Setup(p => p.CreateUploadUrlAsync(
                It.IsAny<string>(),
                command.ContentType.Trim(),
                command.SizeBytes,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(uploadUrl);

        var handler = new CreateFileCommandHandler(
            fileRepository.Object,
            folderRepository.Object,
            userRepository.Object,
            uploadUrlProvider.Object,
            unitOfWork.Object
        );

        var output = await handler.Handle(command, CancellationToken.None);

        fileRepository.Verify(
            r => r.InsertAsync(It.IsAny<DomainEntity.FileItem>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        uploadUrlProvider.Verify(
            p => p.CreateUploadUrlAsync(
                It.IsAny<string>(),
                command.ContentType.Trim(),
                command.SizeBytes,
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        output.Should().NotBeNull();
        output.Name.Should().Be(command.Name.Trim());
        output.ContentType.Should().Be(command.ContentType.Trim());
        output.SizeBytes.Should().Be(command.SizeBytes);
        output.FolderId.Should().Be(folder.Id);
        output.UserId.Should().Be(userId);
        output.UploadUrl.Should().Be(uploadUrl);
        output.Id.Should().NotBe(Guid.Empty);
        output.StorageKey.Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = nameof(ThrowWhenUserNotFound))]
    [Trait("Use Cases", "CreateFile - Handler")]
    public async Task ThrowWhenUserNotFound()
    {
        var userRepository = _fixture.GetUserRepositoryMock();
        var command = _fixture.GetValidCommand();

        userRepository
            .Setup(r => r.ExistsAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new CreateFileCommandHandler(
            _fixture.GetFileRepositoryMock().Object,
            _fixture.GetFolderRepositoryMock().Object,
            userRepository.Object,
            _fixture.GetUploadUrlProviderMock().Object,
            _fixture.GetUnitOfWorkMock().Object
        );

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"User '{command.UserId}' was not found.");
    }

    [Fact(DisplayName = nameof(ThrowWhenFolderNotFound))]
    [Trait("Use Cases", "CreateFile - Handler")]
    public async Task ThrowWhenFolderNotFound()
    {
        var folderRepository = _fixture.GetFolderRepositoryMock();
        var userRepository = _fixture.GetUserRepositoryMock();
        var userId = _fixture.GetValidUserId();
        var command = _fixture.GetValidCommand(userId);

        userRepository
            .Setup(r => r.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        folderRepository
            .Setup(r => r.GetByIdAsync(command.FolderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainEntity.Folder?)null);

        var handler = new CreateFileCommandHandler(
            _fixture.GetFileRepositoryMock().Object,
            folderRepository.Object,
            userRepository.Object,
            _fixture.GetUploadUrlProviderMock().Object,
            _fixture.GetUnitOfWorkMock().Object
        );

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Folder '{command.FolderId}' was not found.");
    }

    [Fact(DisplayName = nameof(ThrowWhenFolderBelongsToAnotherUser))]
    [Trait("Use Cases", "CreateFile - Handler")]
    public async Task ThrowWhenFolderBelongsToAnotherUser()
    {
        var folderRepository = _fixture.GetFolderRepositoryMock();
        var userRepository = _fixture.GetUserRepositoryMock();
        var userId = _fixture.GetValidUserId();
        var folder = _fixture.GetExampleFolder(_fixture.GetValidUserId());
        var command = _fixture.GetValidCommand(userId, folder.Id);

        userRepository
            .Setup(r => r.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        folderRepository
            .Setup(r => r.GetByIdAsync(folder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(folder);

        var handler = new CreateFileCommandHandler(
            _fixture.GetFileRepositoryMock().Object,
            folderRepository.Object,
            userRepository.Object,
            _fixture.GetUploadUrlProviderMock().Object,
            _fixture.GetUnitOfWorkMock().Object
        );

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<ApplicationValidationException>()
            .WithMessage("Folder must belong to the same user.");
    }

    [Fact(DisplayName = nameof(ThrowWhenFileNameAlreadyExists))]
    [Trait("Use Cases", "CreateFile - Handler")]
    public async Task ThrowWhenFileNameAlreadyExists()
    {
        var fileRepository = _fixture.GetFileRepositoryMock();
        var folderRepository = _fixture.GetFolderRepositoryMock();
        var userRepository = _fixture.GetUserRepositoryMock();
        var userId = _fixture.GetValidUserId();
        var folder = _fixture.GetExampleFolder(userId);
        var command = _fixture.GetValidCommand(userId, folder.Id);

        userRepository
            .Setup(r => r.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        folderRepository
            .Setup(r => r.GetByIdAsync(folder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(folder);

        fileRepository
            .Setup(r => r.ExistsByNameAsync(
                userId,
                folder.Id,
                command.Name.Trim(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(true);

        var handler = new CreateFileCommandHandler(
            fileRepository.Object,
            folderRepository.Object,
            userRepository.Object,
            _fixture.GetUploadUrlProviderMock().Object,
            _fixture.GetUnitOfWorkMock().Object
        );

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage($"File '{command.Name.Trim()}' already exists in this folder.");
    }
}
