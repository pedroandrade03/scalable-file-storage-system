using StorageSystem.IntegrationTests.Base;

namespace StorageSystem.IntegrationTests.Application.UseCases.Folders.ListFolderContents;

[CollectionDefinition(nameof(ListFolderContentsTestFixture))]
public class ListFolderContentsTestFixtureCollection : ICollectionFixture<ListFolderContentsTestFixture> { }

public class ListFolderContentsTestFixture : BaseFixture { }
