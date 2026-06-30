using StorageSystem.Application.UseCases.Folders.CreateFolder;
using StorageSystem.IntegrationTests.Base;

namespace StorageSystem.IntegrationTests.Application.UseCases.Folders.CreateFolder;

[CollectionDefinition(nameof(CreateFolderTestFixture))]
public class CreateFolderTestFixtureCollection : ICollectionFixture<CreateFolderTestFixture> { }

public class CreateFolderTestFixture : BaseFixture
{
    public CreateFolderCommand GetValidCommand(Guid userId, Guid? parentFolderId = null) =>
        new(GetValidFolderName(), userId, parentFolderId);
}
