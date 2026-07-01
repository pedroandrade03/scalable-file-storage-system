using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StorageSystem.Infrastructure.Data.EF.Persistence.UnitOfWork;
using Repository = StorageSystem.Infrastructure.Data.EF.Repositories;

namespace StorageSystem.IntegrationTests.Infrastructure.Repositories.FolderRepository;

[Collection(nameof(FolderRepositoryTestFixture))]
public class FolderRepositoryTest
{
    private readonly FolderRepositoryTestFixture _fixture;

    public FolderRepositoryTest(FolderRepositoryTestFixture fixture) => _fixture = fixture;

    [Fact(DisplayName = nameof(Insert))]
    [Trait("Integration/Infrastructure", "FolderRepository - Repositories")]
    public async Task Insert()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleFolder = _fixture.GetExampleFolder();
        var repository = new Repository.FolderRepository(dbContext);

        await repository.InsertAsync(exampleFolder, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var dbFolder = await _fixture.CreateDbContext(true)
            .Folders.FindAsync(exampleFolder.Id);

        dbFolder.Should().NotBeNull();
        dbFolder!.Name.Should().Be(exampleFolder.Name);
        dbFolder.UserId.Should().Be(exampleFolder.UserId);
        dbFolder.ParentFolderId.Should().Be(exampleFolder.ParentFolderId);
    }

    [Fact(DisplayName = nameof(GetById))]
    [Trait("Integration/Infrastructure", "FolderRepository - Repositories")]
    public async Task GetById()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleFolder = _fixture.GetExampleFolder();
        var foldersList = _fixture.GetExampleFoldersList(exampleFolder.UserId);
        foldersList.Add(exampleFolder);

        await dbContext.Folders.AddRangeAsync(foldersList);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FolderRepository(_fixture.CreateDbContext(true));

        var dbFolder = await repository.GetByIdAsync(exampleFolder.Id, CancellationToken.None);

        dbFolder.Should().NotBeNull();
        dbFolder!.Id.Should().Be(exampleFolder.Id);
        dbFolder.Name.Should().Be(exampleFolder.Name);
        dbFolder.UserId.Should().Be(exampleFolder.UserId);
    }

    [Fact(DisplayName = nameof(GetByIdReturnsNullWhenNotFound))]
    [Trait("Integration/Infrastructure", "FolderRepository - Repositories")]
    public async Task GetByIdReturnsNullWhenNotFound()
    {
        var dbContext = _fixture.CreateDbContext();
        await dbContext.Folders.AddRangeAsync(_fixture.GetExampleFoldersList(Guid.NewGuid()));
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FolderRepository(_fixture.CreateDbContext(true));

        var dbFolder = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        dbFolder.Should().BeNull();
    }

    [Fact(DisplayName = nameof(GetByIdAndUserIdReturnsFolderWhenOwnedByUser))]
    [Trait("Integration/Infrastructure", "FolderRepository - Repositories")]
    public async Task GetByIdAndUserIdReturnsFolderWhenOwnedByUser()
    {
        var dbContext = _fixture.CreateDbContext();
        var userId = Guid.NewGuid();
        var folder = _fixture.GetExampleFolder(userId);
        await dbContext.Folders.AddAsync(folder);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FolderRepository(_fixture.CreateDbContext(true));

        var dbFolder = await repository.GetByIdAndUserIdAsync(
            folder.Id,
            userId,
            CancellationToken.None
        );

        dbFolder.Should().NotBeNull();
        dbFolder!.Id.Should().Be(folder.Id);
        dbFolder.UserId.Should().Be(userId);
    }

    [Fact(DisplayName = nameof(GetByIdAndUserIdReturnsNullWhenFolderBelongsToAnotherUser))]
    [Trait("Integration/Infrastructure", "FolderRepository - Repositories")]
    public async Task GetByIdAndUserIdReturnsNullWhenFolderBelongsToAnotherUser()
    {
        var dbContext = _fixture.CreateDbContext();
        var folder = _fixture.GetExampleFolder(Guid.NewGuid());
        await dbContext.Folders.AddAsync(folder);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FolderRepository(_fixture.CreateDbContext(true));

        var dbFolder = await repository.GetByIdAndUserIdAsync(
            folder.Id,
            Guid.NewGuid(),
            CancellationToken.None
        );

        dbFolder.Should().BeNull();
    }

    [Fact(DisplayName = nameof(GetByIdAndUserIdReturnsNullWhenFolderDoesNotExist))]
    [Trait("Integration/Infrastructure", "FolderRepository - Repositories")]
    public async Task GetByIdAndUserIdReturnsNullWhenFolderDoesNotExist()
    {
        var dbContext = _fixture.CreateDbContext();
        await dbContext.Folders.AddAsync(_fixture.GetExampleFolder());
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FolderRepository(_fixture.CreateDbContext(true));

        var dbFolder = await repository.GetByIdAndUserIdAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            CancellationToken.None
        );

        dbFolder.Should().BeNull();
    }

    [Fact(DisplayName = nameof(ExistsByNameReturnsTrueWhenPresent))]
    [Trait("Integration/Infrastructure", "FolderRepository - Repositories")]
    public async Task ExistsByNameReturnsTrueWhenPresent()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleFolder = _fixture.GetExampleFolder();
        await dbContext.Folders.AddAsync(exampleFolder);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FolderRepository(_fixture.CreateDbContext(true));

        var exists = await repository.ExistsByNameAsync(
            exampleFolder.UserId,
            exampleFolder.ParentFolderId,
            exampleFolder.Name,
            CancellationToken.None
        );

        exists.Should().BeTrue();
    }

    [Fact(DisplayName = nameof(ExistsByNameReturnsFalseWhenAbsent))]
    [Trait("Integration/Infrastructure", "FolderRepository - Repositories")]
    public async Task ExistsByNameReturnsFalseWhenAbsent()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleFolder = _fixture.GetExampleFolder();
        await dbContext.Folders.AddAsync(exampleFolder);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FolderRepository(_fixture.CreateDbContext(true));

        var exists = await repository.ExistsByNameAsync(
            exampleFolder.UserId,
            exampleFolder.ParentFolderId,
            "non-existing-folder-name",
            CancellationToken.None
        );

        exists.Should().BeFalse();
    }

    [Fact(DisplayName = nameof(HasSubFoldersReturnsTrueWhenOwnedChildExists))]
    [Trait("Integration/Infrastructure", "FolderRepository - Repositories")]
    public async Task HasSubFoldersReturnsTrueWhenOwnedChildExists()
    {
        var dbContext = _fixture.CreateDbContext();
        var userId = Guid.NewGuid();
        var parent = _fixture.GetExampleFolder(userId);
        var child = _fixture.GetExampleFolder(userId, parent.Id);
        await dbContext.Folders.AddRangeAsync(parent, child);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FolderRepository(_fixture.CreateDbContext(true));

        var hasSubFolders = await repository.HasSubFoldersAsync(
            parent.Id,
            userId,
            CancellationToken.None
        );

        hasSubFolders.Should().BeTrue();
    }

    [Fact(DisplayName = nameof(HasSubFoldersReturnsFalseWhenFolderHasNoChildren))]
    [Trait("Integration/Infrastructure", "FolderRepository - Repositories")]
    public async Task HasSubFoldersReturnsFalseWhenFolderHasNoChildren()
    {
        var dbContext = _fixture.CreateDbContext();
        var userId = Guid.NewGuid();
        var parent = _fixture.GetExampleFolder(userId);
        await dbContext.Folders.AddAsync(parent);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FolderRepository(_fixture.CreateDbContext(true));

        var hasSubFolders = await repository.HasSubFoldersAsync(
            parent.Id,
            userId,
            CancellationToken.None
        );

        hasSubFolders.Should().BeFalse();
    }

    [Fact(DisplayName = nameof(HasSubFoldersReturnsFalseForAnotherUsersChild))]
    [Trait("Integration/Infrastructure", "FolderRepository - Repositories")]
    public async Task HasSubFoldersReturnsFalseForAnotherUsersChild()
    {
        var dbContext = _fixture.CreateDbContext();
        var ownerUserId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();
        var parent = _fixture.GetExampleFolder(ownerUserId);
        var anotherUsersChild = _fixture.GetExampleFolder(anotherUserId, parent.Id);
        await dbContext.Folders.AddRangeAsync(parent, anotherUsersChild);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FolderRepository(_fixture.CreateDbContext(true));

        var hasSubFolders = await repository.HasSubFoldersAsync(
            parent.Id,
            ownerUserId,
            CancellationToken.None
        );

        hasSubFolders.Should().BeFalse();
    }

    [Fact(DisplayName = nameof(DeleteRemovesFolderAfterCommit))]
    [Trait("Integration/Infrastructure", "FolderRepository - Repositories")]
    public async Task DeleteRemovesFolderAfterCommit()
    {
        var dbContext = _fixture.CreateDbContext();
        var folder = _fixture.GetExampleFolder();
        await dbContext.Folders.AddAsync(folder);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FolderRepository(dbContext);
        var unitOfWork = new EfUnitOfWork(dbContext);

        await repository.DeleteAsync(folder, CancellationToken.None);
        await unitOfWork.CommitAsync(CancellationToken.None);

        var deletedFolder = await _fixture.CreateDbContext(true)
            .Folders.FindAsync(folder.Id);

        deletedFolder.Should().BeNull();
    }
}
