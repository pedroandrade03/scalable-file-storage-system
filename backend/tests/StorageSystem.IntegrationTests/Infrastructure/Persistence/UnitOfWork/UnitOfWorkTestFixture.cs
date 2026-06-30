using StorageSystem.IntegrationTests.Base;

namespace StorageSystem.IntegrationTests.Infrastructure.Persistence.UnitOfWork;

[CollectionDefinition(nameof(UnitOfWorkTestFixture))]
public class UnitOfWorkTestFixtureCollection : ICollectionFixture<UnitOfWorkTestFixture> { }

public class UnitOfWorkTestFixture : BaseFixture { }
