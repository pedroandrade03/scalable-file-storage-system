using StorageSystem.IntegrationTests.Base;

namespace StorageSystem.IntegrationTests.Infrastructure.Repositories.UserRepository;

[CollectionDefinition(nameof(UserRepositoryTestFixture))]
public class UserRepositoryTestFixtureCollection : ICollectionFixture<UserRepositoryTestFixture> { }

public class UserRepositoryTestFixture : BaseFixture { }
