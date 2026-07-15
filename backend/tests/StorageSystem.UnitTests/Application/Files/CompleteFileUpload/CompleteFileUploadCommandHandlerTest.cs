using FluentAssertions;
using Moq;
using StorageSystem.Application.Exceptions;
using StorageSystem.Application.Interfaces;
using StorageSystem.Application.UseCases.Files.CompleteFileUpload;
using StorageSystem.Domain.Entities;
using StorageSystem.Domain.Enums;

namespace StorageSystem.UnitTests.Application.Files.CompleteFileUpload;

[Collection(nameof(CompleteFileUploadTestFixture))]
public class CompleteFileUploadCommandHandlerTest
{
    private readonly CompleteFileUploadTestFixture _fixture;

    public CompleteFileUploadCommandHandlerTest(CompleteFileUploadTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(CompleteOwnedFileUpload))]
    [Trait("Use Cases", "CompleteFileUpload - Handler")]
    public async Task CompleteOwnedFileUpload()
    {
        var fileRepository = _fixture.GetFileRepositoryMock();
        var uploadCompleter = _fixture.GetUploadCompleterMock();
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

        var handler = new CompleteFileUploadCommandHandler(
            fileRepository.Object,
            uploadCompleter.Object,
            unitOfWork.Object
        );

        var output = await handler.Handle(command, CancellationToken.None);

        uploadCompleter.Verify(
            completer => completer.CompleteMultipartUploadAsync(
                file.StorageKey,
                command.UploadId,
                command.Parts,
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        file.Status.Should().Be(FileStatus.Available);
        output.Id.Should().Be(file.Id);
        output.Status.Should().Be(FileStatus.Available);
    }

    [Fact(DisplayName = nameof(ThrowWhenFileNotFound))]
    [Trait("Use Cases", "CompleteFileUpload - Handler")]
    public async Task ThrowWhenFileNotFound()
    {
        var fileRepository = _fixture.GetFileRepositoryMock();
        var uploadCompleter = _fixture.GetUploadCompleterMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var command = _fixture.GetValidCommand();

        fileRepository
            .Setup(r => r.GetByIdAndUserIdAsync(
                command.FileId,
                command.UserId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((FileItem?)null);

        var handler = new CompleteFileUploadCommandHandler(
            fileRepository.Object,
            uploadCompleter.Object,
            unitOfWork.Object
        );

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"File '{command.FileId}' was not found.");

        uploadCompleter.Verify(
            completer => completer.CompleteMultipartUploadAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<CompletedMultipartUploadPart>>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
        unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
