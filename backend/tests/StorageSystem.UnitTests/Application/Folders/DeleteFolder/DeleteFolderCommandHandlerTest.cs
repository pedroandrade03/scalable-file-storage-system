using FluentAssertions;
using Moq;
using StorageSystem.Application.Exceptions;
using StorageSystem.Application.UseCases.Folders.DeleteFolder;
using DomainEntity = StorageSystem.Domain.Entities;

namespace StorageSystem.UnitTests.Application.Folders.DeleteFolder;

[Collection(nameof(DeleteFolderTestFixture))]
public class DeleteFolderCommandHandlerTest
{
    private readonly DeleteFolderTestFixture _fixture;

    public DeleteFolderCommandHandlerTest(DeleteFolderTestFixture fixture) => _fixture = fixture;

    [Fact(DisplayName = nameof(DeleteOwnedEmptyFolder))]
    [Trait("Use Cases", "DeleteFolder - Handler")]
    public async Task DeleteOwnedEmptyFolder()
    {
        var folderRepository = _fixture.GetFolderRepositoryMock();
        var fileRepository = _fixture.GetFileRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var userId = _fixture.GetValidUserId();
        var folder = _fixture.GetExampleFolder(userId);
        var command = _fixture.GetValidCommand(folder.Id, userId);

        folderRepository
            .Setup(r => r.GetByIdAndUserIdAsync(
                command.FolderId,
                command.UserId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(folder);

        fileRepository
            .Setup(r => r.ExistsInFolderAsync(
                command.FolderId,
                command.UserId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(false);

        folderRepository
            .Setup(r => r.HasSubFoldersAsync(
                command.FolderId,
                command.UserId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(false);

        var handler = new DeleteFolderCommandHandler(
            folderRepository.Object,
            fileRepository.Object,
            unitOfWork.Object
        );

        await handler.Handle(command, CancellationToken.None);

        folderRepository.Verify(
            r => r.DeleteAsync(
                It.Is<DomainEntity.Folder>(item => item.Id == folder.Id),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = nameof(ThrowWhenFolderNotFound))]
    [Trait("Use Cases", "DeleteFolder - Handler")]
    public async Task ThrowWhenFolderNotFound()
    {
        var folderRepository = _fixture.GetFolderRepositoryMock();
        var fileRepository = _fixture.GetFileRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var command = _fixture.GetValidCommand();

        folderRepository
            .Setup(r => r.GetByIdAndUserIdAsync(
                command.FolderId,
                command.UserId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((DomainEntity.Folder?)null);

        var handler = new DeleteFolderCommandHandler(
            folderRepository.Object,
            fileRepository.Object,
            unitOfWork.Object
        );

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Folder '{command.FolderId}' was not found.");

        fileRepository.Verify(
            r => r.ExistsInFolderAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
        folderRepository.Verify(
            r => r.HasSubFoldersAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
        folderRepository.Verify(
            r => r.DeleteAsync(It.IsAny<DomainEntity.Folder>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = nameof(ThrowWhenFolderContainsFiles))]
    [Trait("Use Cases", "DeleteFolder - Handler")]
    public async Task ThrowWhenFolderContainsFiles()
    {
        var folderRepository = _fixture.GetFolderRepositoryMock();
        var fileRepository = _fixture.GetFileRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var userId = _fixture.GetValidUserId();
        var folder = _fixture.GetExampleFolder(userId);
        var command = _fixture.GetValidCommand(folder.Id, userId);

        folderRepository
            .Setup(r => r.GetByIdAndUserIdAsync(
                command.FolderId,
                command.UserId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(folder);

        fileRepository
            .Setup(r => r.ExistsInFolderAsync(
                command.FolderId,
                command.UserId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(true);

        var handler = new DeleteFolderCommandHandler(
            folderRepository.Object,
            fileRepository.Object,
            unitOfWork.Object
        );

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage($"Folder '{command.FolderId}' cannot be deleted because it contains files.");

        folderRepository.Verify(
            r => r.HasSubFoldersAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
        folderRepository.Verify(
            r => r.DeleteAsync(It.IsAny<DomainEntity.Folder>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = nameof(ThrowWhenFolderContainsSubFolders))]
    [Trait("Use Cases", "DeleteFolder - Handler")]
    public async Task ThrowWhenFolderContainsSubFolders()
    {
        var folderRepository = _fixture.GetFolderRepositoryMock();
        var fileRepository = _fixture.GetFileRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var userId = _fixture.GetValidUserId();
        var folder = _fixture.GetExampleFolder(userId);
        var command = _fixture.GetValidCommand(folder.Id, userId);

        folderRepository
            .Setup(r => r.GetByIdAndUserIdAsync(
                command.FolderId,
                command.UserId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(folder);

        fileRepository
            .Setup(r => r.ExistsInFolderAsync(
                command.FolderId,
                command.UserId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(false);

        folderRepository
            .Setup(r => r.HasSubFoldersAsync(
                command.FolderId,
                command.UserId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(true);

        var handler = new DeleteFolderCommandHandler(
            folderRepository.Object,
            fileRepository.Object,
            unitOfWork.Object
        );

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage($"Folder '{command.FolderId}' cannot be deleted because it contains subfolders.");

        folderRepository.Verify(
            r => r.DeleteAsync(It.IsAny<DomainEntity.Folder>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
