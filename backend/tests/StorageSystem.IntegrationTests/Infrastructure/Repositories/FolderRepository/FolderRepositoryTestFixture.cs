using StorageSystem.IntegrationTests.Base;

namespace StorageSystem.IntegrationTests.Infrastructure.Repositories.FolderRepository;

[CollectionDefinition(nameof(FolderRepositoryTestFixture))]
public class FolderRepositoryTestFixtureCollection : ICollectionFixture<FolderRepositoryTestFixture> { }

public class FolderRepositoryTestFixture : BaseFixture { }
