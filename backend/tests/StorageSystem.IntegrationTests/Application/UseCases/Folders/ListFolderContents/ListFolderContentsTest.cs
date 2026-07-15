using FluentAssertions;
using StorageSystem.Application.Exceptions;
using StorageSystem.Application.UseCases.Folders.ListFolderContents;

namespace StorageSystem.IntegrationTests.Application.UseCases.Folders.ListFolderContents;

[Collection(nameof(ListFolderContentsTestFixture))]
public class ListFolderContentsTest
{
    private readonly ListFolderContentsTestFixture _fixture;

    public ListFolderContentsTest(ListFolderContentsTestFixture fixture) => _fixture = fixture;

    [Fact(DisplayName = nameof(ListRootFolderContents))]
    [Trait("Integration/Application", "ListFolderContents - Use Cases")]
    public async Task ListRootFolderContents()
    {
        var dbContext = _fixture.CreateDbContext();
        var user = _fixture.GetExampleUser();
        var rootFolder = _fixture.GetExampleFolder(user.Id);
        var anotherRootFolder = _fixture.GetExampleFolder(user.Id);
        var childFolder = _fixture.GetExampleFolder(user.Id, rootFolder.Id);
        var anotherUser = _fixture.GetExampleUser();
        var anotherUsersRootFolder = _fixture.GetExampleFolder(anotherUser.Id);
        await dbContext.Users.AddRangeAsync(user, anotherUser);
        await dbContext.Folders.AddRangeAsync(
            rootFolder,
            anotherRootFolder,
            childFolder,
            anotherUsersRootFolder
        );
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = new ListFolderContentsQueryHandler(
            _fixture.CreateFolderRepository(_fixture.CreateDbContext(true)),
            _fixture.CreateFileRepository(_fixture.CreateDbContext(true))
        );

        var output = await handler.Handle(
            new ListFolderContentsQuery(user.Id, null),
            CancellationToken.None
        );

        output.ParentFolderId.Should().BeNull();
        output.Folders.Select(folder => folder.Id).Should()
            .BeEquivalentTo([rootFolder.Id, anotherRootFolder.Id]);
        output.Files.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(ListChildFolderContents))]
    [Trait("Integration/Application", "ListFolderContents - Use Cases")]
    public async Task ListChildFolderContents()
    {
        var dbContext = _fixture.CreateDbContext();
        var user = _fixture.GetExampleUser();
        var parentFolder = _fixture.GetExampleFolder(user.Id);
        var childFolder = _fixture.GetExampleFolder(user.Id, parentFolder.Id);
        var siblingFolder = _fixture.GetExampleFolder(user.Id);
        var file = _fixture.GetExampleFile(user.Id, parentFolder.Id);
        var siblingFile = _fixture.GetExampleFile(user.Id, siblingFolder.Id);
        var anotherUser = _fixture.GetExampleUser();
        var anotherUsersChildFolder = _fixture.GetExampleFolder(anotherUser.Id, parentFolder.Id);
        var anotherUsersFile = _fixture.GetExampleFile(anotherUser.Id, parentFolder.Id);
        await dbContext.Users.AddRangeAsync(user, anotherUser);
        await dbContext.Folders.AddRangeAsync(
            parentFolder,
            childFolder,
            siblingFolder,
            anotherUsersChildFolder
        );
        await dbContext.Files.AddRangeAsync(file, siblingFile, anotherUsersFile);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = new ListFolderContentsQueryHandler(
            _fixture.CreateFolderRepository(_fixture.CreateDbContext(true)),
            _fixture.CreateFileRepository(_fixture.CreateDbContext(true))
        );

        var output = await handler.Handle(
            new ListFolderContentsQuery(user.Id, parentFolder.Id),
            CancellationToken.None
        );

        output.ParentFolderId.Should().Be(parentFolder.Id);
        output.Folders.Should().ContainSingle();
        output.Folders[0].Id.Should().Be(childFolder.Id);
        output.Files.Should().ContainSingle();
        output.Files[0].Id.Should().Be(file.Id);
    }

    [Fact(DisplayName = nameof(ThrowWhenParentFolderBelongsToAnotherUser))]
    [Trait("Integration/Application", "ListFolderContents - Use Cases")]
    public async Task ThrowWhenParentFolderBelongsToAnotherUser()
    {
        var dbContext = _fixture.CreateDbContext();
        var owner = _fixture.GetExampleUser();
        var requester = _fixture.GetExampleUser();
        var parentFolder = _fixture.GetExampleFolder(owner.Id);
        await dbContext.Users.AddRangeAsync(owner, requester);
        await dbContext.Folders.AddAsync(parentFolder);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = new ListFolderContentsQueryHandler(
            _fixture.CreateFolderRepository(_fixture.CreateDbContext(true)),
            _fixture.CreateFileRepository(_fixture.CreateDbContext(true))
        );

        var action = () => handler.Handle(
            new ListFolderContentsQuery(requester.Id, parentFolder.Id),
            CancellationToken.None
        );

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Folder '{parentFolder.Id}' was not found.");
    }
}
