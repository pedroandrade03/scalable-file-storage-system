using FluentAssertions;
using Repository = StorageSystem.Infrastructure.Repositories;

namespace StorageSystem.IntegrationTests.Infrastructure.Repositories.FileRepository;

[Collection(nameof(FileRepositoryTestFixture))]
public class FileRepositoryTest
{
    private readonly FileRepositoryTestFixture _fixture;

    public FileRepositoryTest(FileRepositoryTestFixture fixture) => _fixture = fixture;

    [Fact(DisplayName = nameof(Insert))]
    [Trait("Integration/Infrastructure", "FileRepository - Repositories")]
    public async Task Insert()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleFile = _fixture.GetExampleFile();
        var repository = new Repository.FileRepository(dbContext);

        await repository.InsertAsync(exampleFile, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var dbFile = await _fixture.CreateDbContext(true)
            .Files.FindAsync(exampleFile.Id);

        dbFile.Should().NotBeNull();
        dbFile!.Name.Should().Be(exampleFile.Name);
        dbFile.ContentType.Should().Be(exampleFile.ContentType);
        dbFile.SizeBytes.Should().Be(exampleFile.SizeBytes);
        dbFile.StorageKey.Should().Be(exampleFile.StorageKey);
        dbFile.FolderId.Should().Be(exampleFile.FolderId);
        dbFile.UserId.Should().Be(exampleFile.UserId);
    }

    [Fact(DisplayName = nameof(GetById))]
    [Trait("Integration/Infrastructure", "FileRepository - Repositories")]
    public async Task GetById()
    {
        var dbContext = _fixture.CreateDbContext();
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var exampleFile = _fixture.GetExampleFile(userId, folderId);
        await dbContext.Files.AddAsync(exampleFile);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FileRepository(_fixture.CreateDbContext(true));

        var dbFile = await repository.GetByIdAsync(exampleFile.Id, CancellationToken.None);

        dbFile.Should().NotBeNull();
        dbFile!.Id.Should().Be(exampleFile.Id);
        dbFile.StorageKey.Should().Be(exampleFile.StorageKey);
    }

    [Fact(DisplayName = nameof(GetByIdReturnsNullWhenNotFound))]
    [Trait("Integration/Infrastructure", "FileRepository - Repositories")]
    public async Task GetByIdReturnsNullWhenNotFound()
    {
        var dbContext = _fixture.CreateDbContext();
        await dbContext.Files.AddAsync(_fixture.GetExampleFile());
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FileRepository(_fixture.CreateDbContext(true));

        var dbFile = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        dbFile.Should().BeNull();
    }

    [Fact(DisplayName = nameof(ExistsByNameReturnsTrueWhenPresent))]
    [Trait("Integration/Infrastructure", "FileRepository - Repositories")]
    public async Task ExistsByNameReturnsTrueWhenPresent()
    {
        var dbContext = _fixture.CreateDbContext();
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var exampleFile = _fixture.GetExampleFile(userId, folderId);
        await dbContext.Files.AddAsync(exampleFile);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FileRepository(_fixture.CreateDbContext(true));

        var exists = await repository.ExistsByNameAsync(
            userId,
            folderId,
            exampleFile.Name,
            CancellationToken.None
        );

        exists.Should().BeTrue();
    }

    [Fact(DisplayName = nameof(ExistsByNameReturnsFalseWhenAbsent))]
    [Trait("Integration/Infrastructure", "FileRepository - Repositories")]
    public async Task ExistsByNameReturnsFalseWhenAbsent()
    {
        var dbContext = _fixture.CreateDbContext();
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var exampleFile = _fixture.GetExampleFile(userId, folderId);
        await dbContext.Files.AddAsync(exampleFile);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FileRepository(_fixture.CreateDbContext(true));

        var exists = await repository.ExistsByNameAsync(
            userId,
            folderId,
            "non-existing-file.pdf",
            CancellationToken.None
        );

        exists.Should().BeFalse();
    }
}
