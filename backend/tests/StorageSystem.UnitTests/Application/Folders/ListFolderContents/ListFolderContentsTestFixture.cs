using StorageSystem.UnitTests.Application.Files.Common;
using StorageSystem.Application.UseCases.Folders.ListFolderContents;

namespace StorageSystem.UnitTests.Application.Folders.ListFolderContents;

[CollectionDefinition(nameof(ListFolderContentsTestFixture))]
public class ListFolderContentsTestFixtureCollection : ICollectionFixture<ListFolderContentsTestFixture> { }

public class ListFolderContentsTestFixture : FileUseCasesBaseFixture
{
    public ListFolderContentsQuery GetValidQuery(
        Guid? userId = null,
        Guid? parentFolderId = null
    ) => new(userId ?? GetValidUserId(), parentFolderId);
}
