using StorageSystem.IntegrationTests.Base;

namespace StorageSystem.IntegrationTests.Infrastructure.Repositories.FileRepository;

[CollectionDefinition(nameof(FileRepositoryTestFixture))]
public class FileRepositoryTestFixtureCollection : ICollectionFixture<FileRepositoryTestFixture> { }

public class FileRepositoryTestFixture : BaseFixture { }
