using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using StorageSystem.Application.Exceptions;
using StorageSystem.Application.Interfaces;
using StorageSystem.Application.UseCases.Files.DeleteFile;

namespace StorageSystem.IntegrationTests.Application.UseCases.Files.DeleteFile;

[Collection(nameof(DeleteFileTestFixture))]
public class DeleteFileTest
{
    private readonly DeleteFileTestFixture _fixture;

    public DeleteFileTest(DeleteFileTestFixture fixture) => _fixture = fixture;

    [Fact(DisplayName = nameof(DeleteFile))]
    [Trait("Integration/Application", "DeleteFile - Use Cases")]
    public async Task DeleteFile()
    {
        var dbContext = _fixture.CreateDbContext();
        var user = _fixture.GetExampleUser();
        var folder = _fixture.GetExampleFolder(user.Id);
        var file = _fixture.GetExampleFile(user.Id, folder.Id);
        await dbContext.Users.AddAsync(user);
        await dbContext.Folders.AddAsync(folder);
        await dbContext.Files.AddAsync(file);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var fileStorageRemover = new Mock<IFileStorageRemover>();
        var handler = new DeleteFileCommandHandler(
            _fixture.CreateFileRepository(dbContext),
            fileStorageRemover.Object,
            _fixture.CreateUnitOfWork(dbContext)
        );

        await handler.Handle(_fixture.GetValidCommand(file.Id, user.Id), CancellationToken.None);

        fileStorageRemover.Verify(
            remover => remover.DeleteAsync(file.StorageKey, It.IsAny<CancellationToken>()),
            Times.Once
        );

        var dbFile = await _fixture.CreateDbContext(true)
            .Files.AsNoTracking()
            .SingleOrDefaultAsync(dbFile => dbFile.Id == file.Id);
        dbFile.Should().BeNull();
    }

    [Fact(DisplayName = nameof(ThrowWhenFileBelongsToAnotherUser))]
    [Trait("Integration/Application", "DeleteFile - Use Cases")]
    public async Task ThrowWhenFileBelongsToAnotherUser()
    {
        var dbContext = _fixture.CreateDbContext();
        var owner = _fixture.GetExampleUser();
        var anotherUser = _fixture.GetExampleUser();
        var folder = _fixture.GetExampleFolder(owner.Id);
        var file = _fixture.GetExampleFile(owner.Id, folder.Id);
        await dbContext.Users.AddRangeAsync(owner, anotherUser);
        await dbContext.Folders.AddAsync(folder);
        await dbContext.Files.AddAsync(file);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var fileStorageRemover = new Mock<IFileStorageRemover>();
        var handler = new DeleteFileCommandHandler(
            _fixture.CreateFileRepository(dbContext),
            fileStorageRemover.Object,
            _fixture.CreateUnitOfWork(dbContext)
        );

        var action = () => handler.Handle(
            _fixture.GetValidCommand(file.Id, anotherUser.Id),
            CancellationToken.None
        );

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"File '{file.Id}' was not found.");

        fileStorageRemover.Verify(
            remover => remover.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        var dbFile = await _fixture.CreateDbContext(true)
            .Files.AsNoTracking()
            .SingleOrDefaultAsync(dbFile => dbFile.Id == file.Id);
        dbFile.Should().NotBeNull();
    }

    [Fact(DisplayName = nameof(ThrowWhenStorageDeletionFails))]
    [Trait("Integration/Application", "DeleteFile - Use Cases")]
    public async Task ThrowWhenStorageDeletionFails()
    {
        var dbContext = _fixture.CreateDbContext();
        var user = _fixture.GetExampleUser();
        var folder = _fixture.GetExampleFolder(user.Id);
        var file = _fixture.GetExampleFile(user.Id, folder.Id);
        await dbContext.Users.AddAsync(user);
        await dbContext.Folders.AddAsync(folder);
        await dbContext.Files.AddAsync(file);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var fileStorageRemover = new Mock<IFileStorageRemover>();
        fileStorageRemover
            .Setup(remover => remover.DeleteAsync(file.StorageKey, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("storage unavailable"));
        var handler = new DeleteFileCommandHandler(
            _fixture.CreateFileRepository(dbContext),
            fileStorageRemover.Object,
            _fixture.CreateUnitOfWork(dbContext)
        );

        var action = () => handler.Handle(_fixture.GetValidCommand(file.Id, user.Id), CancellationToken.None);

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("storage unavailable");

        var dbFile = await _fixture.CreateDbContext(true)
            .Files.AsNoTracking()
            .SingleOrDefaultAsync(dbFile => dbFile.Id == file.Id);
        dbFile.Should().NotBeNull();
    }
}
