using Moq;
using StorageSystem.Application.UseCases.Folders.DeleteFolder;
using StorageSystem.Domain.Repositories;
using StorageSystem.UnitTests.Application.Folders.Common;

namespace StorageSystem.UnitTests.Application.Folders.DeleteFolder;

[CollectionDefinition(nameof(DeleteFolderTestFixture))]
public class DeleteFolderTestFixtureCollection : ICollectionFixture<DeleteFolderTestFixture> { }

public class DeleteFolderTestFixture : FolderUseCasesBaseFixture
{
    public Mock<IFileRepository> GetFileRepositoryMock() => new();

    public DeleteFolderCommand GetValidCommand(
        Guid? folderId = null,
        Guid? userId = null
    ) => new(
        folderId ?? Faker.Random.Guid(),
        userId ?? GetValidUserId()
    );
}
