using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Repository = StorageSystem.Infrastructure.Repositories;

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
}
