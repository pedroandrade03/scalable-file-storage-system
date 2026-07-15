using FluentAssertions;
using StorageSystem.Domain.Enums;
using StorageSystem.Infrastructure.Data.EF.Persistence.UnitOfWork;
using Repository = StorageSystem.Infrastructure.Data.EF.Repositories;

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
        dbFile.Status.Should().Be(FileStatus.PendingUpload);
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

    [Fact(DisplayName = nameof(GetByIdAndUserIdReturnsFileWhenOwnedByUser))]
    [Trait("Integration/Infrastructure", "FileRepository - Repositories")]
    public async Task GetByIdAndUserIdReturnsFileWhenOwnedByUser()
    {
        var dbContext = _fixture.CreateDbContext();
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var exampleFile = _fixture.GetExampleFile(userId, folderId);
        await dbContext.Files.AddAsync(exampleFile);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FileRepository(_fixture.CreateDbContext(true));

        var dbFile = await repository.GetByIdAndUserIdAsync(
            exampleFile.Id,
            userId,
            CancellationToken.None
        );

        dbFile.Should().NotBeNull();
        dbFile!.Id.Should().Be(exampleFile.Id);
        dbFile.UserId.Should().Be(userId);
    }

    [Fact(DisplayName = nameof(GetByIdAndUserIdReturnsNullWhenFileBelongsToAnotherUser))]
    [Trait("Integration/Infrastructure", "FileRepository - Repositories")]
    public async Task GetByIdAndUserIdReturnsNullWhenFileBelongsToAnotherUser()
    {
        var dbContext = _fixture.CreateDbContext();
        var ownerUserId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();
        var exampleFile = _fixture.GetExampleFile(ownerUserId);
        await dbContext.Files.AddAsync(exampleFile);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FileRepository(_fixture.CreateDbContext(true));

        var dbFile = await repository.GetByIdAndUserIdAsync(
            exampleFile.Id,
            anotherUserId,
            CancellationToken.None
        );

        dbFile.Should().BeNull();
    }

    [Fact(DisplayName = nameof(GetByIdAndUserIdReturnsNullWhenFileDoesNotExist))]
    [Trait("Integration/Infrastructure", "FileRepository - Repositories")]
    public async Task GetByIdAndUserIdReturnsNullWhenFileDoesNotExist()
    {
        var dbContext = _fixture.CreateDbContext();
        await dbContext.Files.AddAsync(_fixture.GetExampleFile());
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FileRepository(_fixture.CreateDbContext(true));

        var dbFile = await repository.GetByIdAndUserIdAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            CancellationToken.None
        );

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

    [Fact(DisplayName = nameof(ExistsInFolderReturnsTrueWhenOwnedFolderHasFiles))]
    [Trait("Integration/Infrastructure", "FileRepository - Repositories")]
    public async Task ExistsInFolderReturnsTrueWhenOwnedFolderHasFiles()
    {
        var dbContext = _fixture.CreateDbContext();
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var file = _fixture.GetExampleFile(userId, folderId);
        await dbContext.Files.AddAsync(file);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FileRepository(_fixture.CreateDbContext(true));

        var exists = await repository.ExistsInFolderAsync(
            folderId,
            userId,
            CancellationToken.None
        );

        exists.Should().BeTrue();
    }

    [Fact(DisplayName = nameof(ExistsInFolderReturnsFalseWhenFolderHasNoFiles))]
    [Trait("Integration/Infrastructure", "FileRepository - Repositories")]
    public async Task ExistsInFolderReturnsFalseWhenFolderHasNoFiles()
    {
        var dbContext = _fixture.CreateDbContext();
        await dbContext.Files.AddAsync(_fixture.GetExampleFile());
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FileRepository(_fixture.CreateDbContext(true));

        var exists = await repository.ExistsInFolderAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            CancellationToken.None
        );

        exists.Should().BeFalse();
    }

    [Fact(DisplayName = nameof(ExistsInFolderReturnsFalseForAnotherUsersFile))]
    [Trait("Integration/Infrastructure", "FileRepository - Repositories")]
    public async Task ExistsInFolderReturnsFalseForAnotherUsersFile()
    {
        var dbContext = _fixture.CreateDbContext();
        var folderId = Guid.NewGuid();
        var file = _fixture.GetExampleFile(Guid.NewGuid(), folderId);
        await dbContext.Files.AddAsync(file);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FileRepository(_fixture.CreateDbContext(true));

        var exists = await repository.ExistsInFolderAsync(
            folderId,
            Guid.NewGuid(),
            CancellationToken.None
        );

        exists.Should().BeFalse();
    }

    [Fact(DisplayName = nameof(ListByFolderReturnsOnlyOwnedFilesInFolder))]
    [Trait("Integration/Infrastructure", "FileRepository - Repositories")]
    public async Task ListByFolderReturnsOnlyOwnedFilesInFolder()
    {
        var dbContext = _fixture.CreateDbContext();
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var file = _fixture.GetExampleFile(userId, folderId);
        var anotherFile = _fixture.GetExampleFile(userId, folderId);
        var siblingFolderFile = _fixture.GetExampleFile(userId, Guid.NewGuid());
        var anotherUsersFile = _fixture.GetExampleFile(Guid.NewGuid(), folderId);
        await dbContext.Files.AddRangeAsync(
            file,
            anotherFile,
            siblingFolderFile,
            anotherUsersFile
        );
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FileRepository(_fixture.CreateDbContext(true));

        var files = await repository.ListByFolderAsync(userId, folderId, CancellationToken.None);

        files.Select(listedFile => listedFile.Id).Should()
            .BeEquivalentTo([file.Id, anotherFile.Id]);
    }

    [Fact(DisplayName = nameof(DeleteRemovesMetadataAfterCommit))]
    [Trait("Integration/Infrastructure", "FileRepository - Repositories")]
    public async Task DeleteRemovesMetadataAfterCommit()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleFile = _fixture.GetExampleFile();
        await dbContext.Files.AddAsync(exampleFile);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.FileRepository(dbContext);
        var unitOfWork = new EfUnitOfWork(dbContext);

        await repository.DeleteAsync(exampleFile, CancellationToken.None);
        await unitOfWork.CommitAsync(CancellationToken.None);

        var deletedFile = await _fixture.CreateDbContext(true)
            .Files.FindAsync(exampleFile.Id);

        deletedFile.Should().BeNull();
    }
}
