using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StorageSystem.Application.Exceptions;
using StorageSystem.Application.UseCases.Folders.DeleteFolder;

namespace StorageSystem.IntegrationTests.Application.UseCases.Folders.DeleteFolder;

[Collection(nameof(DeleteFolderTestFixture))]
public class DeleteFolderTest
{
    private readonly DeleteFolderTestFixture _fixture;

    public DeleteFolderTest(DeleteFolderTestFixture fixture) => _fixture = fixture;

    [Fact(DisplayName = nameof(DeleteFolder))]
    [Trait("Integration/Application", "DeleteFolder - Use Cases")]
    public async Task DeleteFolder()
    {
        var dbContext = _fixture.CreateDbContext();
        var user = _fixture.GetExampleUser();
        var folder = _fixture.GetExampleFolder(user.Id);
        await dbContext.Users.AddAsync(user);
        await dbContext.Folders.AddAsync(folder);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteFolderCommandHandler(
            _fixture.CreateFolderRepository(dbContext),
            _fixture.CreateFileRepository(dbContext),
            _fixture.CreateUnitOfWork(dbContext)
        );

        await handler.Handle(_fixture.GetValidCommand(folder.Id, user.Id), CancellationToken.None);

        var dbFolder = await _fixture.CreateDbContext(true)
            .Folders.AsNoTracking()
            .SingleOrDefaultAsync(dbFolder => dbFolder.Id == folder.Id);
        dbFolder.Should().BeNull();
    }

    [Fact(DisplayName = nameof(ThrowWhenFolderBelongsToAnotherUser))]
    [Trait("Integration/Application", "DeleteFolder - Use Cases")]
    public async Task ThrowWhenFolderBelongsToAnotherUser()
    {
        var dbContext = _fixture.CreateDbContext();
        var owner = _fixture.GetExampleUser();
        var anotherUser = _fixture.GetExampleUser();
        var folder = _fixture.GetExampleFolder(owner.Id);
        await dbContext.Users.AddRangeAsync(owner, anotherUser);
        await dbContext.Folders.AddAsync(folder);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteFolderCommandHandler(
            _fixture.CreateFolderRepository(dbContext),
            _fixture.CreateFileRepository(dbContext),
            _fixture.CreateUnitOfWork(dbContext)
        );

        var action = () => handler.Handle(
            _fixture.GetValidCommand(folder.Id, anotherUser.Id),
            CancellationToken.None
        );

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Folder '{folder.Id}' was not found.");

        var dbFolder = await _fixture.CreateDbContext(true)
            .Folders.AsNoTracking()
            .SingleOrDefaultAsync(dbFolder => dbFolder.Id == folder.Id);
        dbFolder.Should().NotBeNull();
    }

    [Fact(DisplayName = nameof(ThrowWhenFolderContainsFiles))]
    [Trait("Integration/Application", "DeleteFolder - Use Cases")]
    public async Task ThrowWhenFolderContainsFiles()
    {
        var dbContext = _fixture.CreateDbContext();
        var user = _fixture.GetExampleUser();
        var folder = _fixture.GetExampleFolder(user.Id);
        var file = _fixture.GetExampleFile(user.Id, folder.Id);
        await dbContext.Users.AddAsync(user);
        await dbContext.Folders.AddAsync(folder);
        await dbContext.Files.AddAsync(file);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteFolderCommandHandler(
            _fixture.CreateFolderRepository(dbContext),
            _fixture.CreateFileRepository(dbContext),
            _fixture.CreateUnitOfWork(dbContext)
        );

        var action = () => handler.Handle(
            _fixture.GetValidCommand(folder.Id, user.Id),
            CancellationToken.None
        );

        await action.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage($"Folder '{folder.Id}' cannot be deleted because it contains files.");

        var assertContext = _fixture.CreateDbContext(true);
        var dbFolder = await assertContext.Folders.AsNoTracking()
            .SingleOrDefaultAsync(dbFolder => dbFolder.Id == folder.Id);
        var dbFile = await assertContext.Files.AsNoTracking()
            .SingleOrDefaultAsync(dbFile => dbFile.Id == file.Id);
        dbFolder.Should().NotBeNull();
        dbFile.Should().NotBeNull();
    }

    [Fact(DisplayName = nameof(ThrowWhenFolderContainsSubFolders))]
    [Trait("Integration/Application", "DeleteFolder - Use Cases")]
    public async Task ThrowWhenFolderContainsSubFolders()
    {
        var dbContext = _fixture.CreateDbContext();
        var user = _fixture.GetExampleUser();
        var parentFolder = _fixture.GetExampleFolder(user.Id);
        var childFolder = _fixture.GetExampleFolder(user.Id, parentFolder.Id);
        await dbContext.Users.AddAsync(user);
        await dbContext.Folders.AddRangeAsync(parentFolder, childFolder);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteFolderCommandHandler(
            _fixture.CreateFolderRepository(dbContext),
            _fixture.CreateFileRepository(dbContext),
            _fixture.CreateUnitOfWork(dbContext)
        );

        var action = () => handler.Handle(
            _fixture.GetValidCommand(parentFolder.Id, user.Id),
            CancellationToken.None
        );

        await action.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage($"Folder '{parentFolder.Id}' cannot be deleted because it contains subfolders.");

        var assertContext = _fixture.CreateDbContext(true);
        var dbParentFolder = await assertContext.Folders.AsNoTracking()
            .SingleOrDefaultAsync(dbFolder => dbFolder.Id == parentFolder.Id);
        var dbChildFolder = await assertContext.Folders.AsNoTracking()
            .SingleOrDefaultAsync(dbFolder => dbFolder.Id == childFolder.Id);
        dbParentFolder.Should().NotBeNull();
        dbChildFolder.Should().NotBeNull();
    }
}
