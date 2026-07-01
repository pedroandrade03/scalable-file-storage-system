using StorageSystem.Application.UseCases.Folders.DeleteFolder;
using StorageSystem.IntegrationTests.Base;

namespace StorageSystem.IntegrationTests.Application.UseCases.Folders.DeleteFolder;

[CollectionDefinition(nameof(DeleteFolderTestFixture))]
public class DeleteFolderTestFixtureCollection : ICollectionFixture<DeleteFolderTestFixture> { }

public class DeleteFolderTestFixture : BaseFixture
{
    public DeleteFolderCommand GetValidCommand(Guid folderId, Guid userId) => new(folderId, userId);
}
