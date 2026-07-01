using FluentAssertions;
using Moq;
using StorageSystem.Application.Exceptions;
using StorageSystem.Application.UseCases.Files.DeleteFile;
using DomainEntity = StorageSystem.Domain.Entities;

namespace StorageSystem.UnitTests.Application.Files.DeleteFile;

[Collection(nameof(DeleteFileTestFixture))]
public class DeleteFileCommandHandlerTest
{
    private readonly DeleteFileTestFixture _fixture;

    public DeleteFileCommandHandlerTest(DeleteFileTestFixture fixture) => _fixture = fixture;

    [Fact(DisplayName = nameof(DeleteOwnedFile))]
    [Trait("Use Cases", "DeleteFile - Handler")]
    public async Task DeleteOwnedFile()
    {
        var fileRepository = _fixture.GetFileRepositoryMock();
        var storageRemover = _fixture.GetStorageRemoverMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var userId = _fixture.GetValidUserId();
        var file = _fixture.GetExampleFile(userId);
        var command = _fixture.GetValidCommand(file.Id, userId);

        fileRepository
            .Setup(r => r.GetByIdAndUserIdAsync(
                command.FileId,
                command.UserId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(file);

        var handler = new DeleteFileCommandHandler(
            fileRepository.Object,
            storageRemover.Object,
            unitOfWork.Object
        );

        await handler.Handle(command, CancellationToken.None);

        storageRemover.Verify(
            s => s.DeleteAsync(file.StorageKey, It.IsAny<CancellationToken>()),
            Times.Once
        );
        fileRepository.Verify(
            r => r.DeleteAsync(
                It.Is<DomainEntity.FileItem>(item => item.Id == file.Id),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = nameof(ThrowWhenFileNotFound))]
    [Trait("Use Cases", "DeleteFile - Handler")]
    public async Task ThrowWhenFileNotFound()
    {
        var fileRepository = _fixture.GetFileRepositoryMock();
        var storageRemover = _fixture.GetStorageRemoverMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var command = _fixture.GetValidCommand();

        fileRepository
            .Setup(r => r.GetByIdAndUserIdAsync(
                command.FileId,
                command.UserId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((DomainEntity.FileItem?)null);

        var handler = new DeleteFileCommandHandler(
            fileRepository.Object,
            storageRemover.Object,
            unitOfWork.Object
        );

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"File '{command.FileId}' was not found.");

        storageRemover.Verify(
            s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        fileRepository.Verify(
            r => r.DeleteAsync(It.IsAny<DomainEntity.FileItem>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = nameof(ThrowWhenStorageDeletionFails))]
    [Trait("Use Cases", "DeleteFile - Handler")]
    public async Task ThrowWhenStorageDeletionFails()
    {
        var fileRepository = _fixture.GetFileRepositoryMock();
        var storageRemover = _fixture.GetStorageRemoverMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var userId = _fixture.GetValidUserId();
        var file = _fixture.GetExampleFile(userId);
        var command = _fixture.GetValidCommand(file.Id, userId);
        var storageException = new InvalidOperationException("Storage delete failed.");

        fileRepository
            .Setup(r => r.GetByIdAndUserIdAsync(
                command.FileId,
                command.UserId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(file);

        storageRemover
            .Setup(s => s.DeleteAsync(file.StorageKey, It.IsAny<CancellationToken>()))
            .ThrowsAsync(storageException);

        var handler = new DeleteFileCommandHandler(
            fileRepository.Object,
            storageRemover.Object,
            unitOfWork.Object
        );

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(storageException.Message);

        fileRepository.Verify(
            r => r.DeleteAsync(It.IsAny<DomainEntity.FileItem>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
