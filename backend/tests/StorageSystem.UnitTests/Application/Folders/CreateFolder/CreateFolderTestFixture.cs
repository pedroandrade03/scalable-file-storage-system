using StorageSystem.Application.UseCases.Folders.CreateFolder;
using StorageSystem.UnitTests.Application.Folders.Common;

namespace StorageSystem.UnitTests.Application.Folders.CreateFolder;

[CollectionDefinition(nameof(CreateFolderTestFixture))]
public class CreateFolderTestFixtureCollection : ICollectionFixture<CreateFolderTestFixture> { }

public class CreateFolderTestFixture : FolderUseCasesBaseFixture
{
    public CreateFolderCommand GetValidCommand(
        Guid? userId = null,
        Guid? parentFolderId = null,
        string? name = null
    ) => new(
        name ?? GetValidFolderName(),
        userId ?? GetValidUserId(),
        parentFolderId
    );
}
