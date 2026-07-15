using FluentAssertions;
using Moq;
using StorageSystem.Application.Exceptions;
using StorageSystem.Application.UseCases.Folders.ListFolderContents;
using StorageSystem.Domain.Enums;
using DomainEntity = StorageSystem.Domain.Entities;

namespace StorageSystem.UnitTests.Application.Folders.ListFolderContents;

[Collection(nameof(ListFolderContentsTestFixture))]
public class ListFolderContentsQueryHandlerTest
{
    private readonly ListFolderContentsTestFixture _fixture;

    public ListFolderContentsQueryHandlerTest(ListFolderContentsTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(ListRootFolders))]
    [Trait("Use Cases", "ListFolderContents - Handler")]
    public async Task ListRootFolders()
    {
        var folderRepository = _fixture.GetFolderRepositoryMock();
        var fileRepository = _fixture.GetFileRepositoryMock();
        var userId = _fixture.GetValidUserId();
        var folders = new List<DomainEntity.Folder>
        {
            _fixture.GetExampleFolder(userId),
            _fixture.GetExampleFolder(userId)
        };
        var query = _fixture.GetValidQuery(userId);

        folderRepository
            .Setup(r => r.ListByParentAsync(userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(folders);

        var handler = new ListFolderContentsQueryHandler(
            folderRepository.Object,
            fileRepository.Object
        );

        var output = await handler.Handle(query, CancellationToken.None);

        output.ParentFolderId.Should().BeNull();
        output.Folders.Should().HaveCount(2);
        output.Folders.Select(folder => folder.Id).Should()
            .BeEquivalentTo(folders.Select(folder => folder.Id));
        output.Files.Should().BeEmpty();

        folderRepository.Verify(
            r => r.ListByParentAsync(userId, null, It.IsAny<CancellationToken>()),
            Times.Once
        );
        fileRepository.Verify(
            r => r.ListByFolderAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact(DisplayName = nameof(ListChildFoldersAndFiles))]
    [Trait("Use Cases", "ListFolderContents - Handler")]
    public async Task ListChildFoldersAndFiles()
    {
        var folderRepository = _fixture.GetFolderRepositoryMock();
        var fileRepository = _fixture.GetFileRepositoryMock();
        var userId = _fixture.GetValidUserId();
        var parentFolder = _fixture.GetExampleFolder(userId);
        var childFolders = new List<DomainEntity.Folder>
        {
            _fixture.GetExampleFolder(userId, parentFolder.Id)
        };
        var files = new List<DomainEntity.FileItem>
        {
            _fixture.GetExampleFile(userId, parentFolder.Id)
        };
        var query = _fixture.GetValidQuery(userId, parentFolder.Id);

        folderRepository
            .Setup(r => r.GetByIdAndUserIdAsync(parentFolder.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentFolder);
        folderRepository
            .Setup(r => r.ListByParentAsync(userId, parentFolder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(childFolders);
        fileRepository
            .Setup(r => r.ListByFolderAsync(userId, parentFolder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(files);

        var handler = new ListFolderContentsQueryHandler(
            folderRepository.Object,
            fileRepository.Object
        );

        var output = await handler.Handle(query, CancellationToken.None);

        output.ParentFolderId.Should().Be(parentFolder.Id);
        output.Folders.Should().ContainSingle();
        output.Folders[0].Id.Should().Be(childFolders[0].Id);
        output.Files.Should().ContainSingle();
        output.Files[0].Id.Should().Be(files[0].Id);
        output.Files[0].Status.Should().Be(FileStatus.PendingUpload);
    }

    [Fact(DisplayName = nameof(ThrowWhenParentFolderNotFound))]
    [Trait("Use Cases", "ListFolderContents - Handler")]
    public async Task ThrowWhenParentFolderNotFound()
    {
        var folderRepository = _fixture.GetFolderRepositoryMock();
        var fileRepository = _fixture.GetFileRepositoryMock();
        var query = _fixture.GetValidQuery(parentFolderId: Guid.NewGuid());

        folderRepository
            .Setup(r => r.GetByIdAndUserIdAsync(
                query.ParentFolderId!.Value,
                query.UserId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((DomainEntity.Folder?)null);

        var handler = new ListFolderContentsQueryHandler(
            folderRepository.Object,
            fileRepository.Object
        );

        var action = () => handler.Handle(query, CancellationToken.None);

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Folder '{query.ParentFolderId}' was not found.");

        folderRepository.Verify(
            r => r.ListByParentAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        fileRepository.Verify(
            r => r.ListByFolderAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
