using DomainEntity = StorageSystem.Domain.Entities;
using StorageSystem.UnitTests.Common;

namespace StorageSystem.UnitTests.Domain.Entities.User;

[CollectionDefinition(nameof(UserTestFixture))]
public class UserFixtureCollection : ICollectionFixture<UserTestFixture> { }

public class UserTestFixture : BaseFixture
{
    public string GetValidUserName() => Faker.Name.FullName();

    public string GetValidUserEmail() => Faker.Internet.Email();

    public string GetValidExternalProvider() => "keycloak";

    public string GetValidExternalSubject() => Faker.Random.Guid().ToString();

    public DomainEntity.User GetValidUser() =>
        new(
            GetValidUserName(),
            GetValidUserEmail(),
            GetValidExternalProvider(),
            GetValidExternalSubject()
        );
}
