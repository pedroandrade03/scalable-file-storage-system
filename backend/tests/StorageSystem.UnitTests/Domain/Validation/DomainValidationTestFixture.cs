using StorageSystem.UnitTests.Common;

namespace StorageSystem.UnitTests.Domain.Validation;

[CollectionDefinition(nameof(DomainValidationTestFixture))]
public class DomainValidationTestFixtureCollection : ICollectionFixture<DomainValidationTestFixture> { }

public class DomainValidationTestFixture : BaseFixture
{
    public string GetValidFieldName() => Faker.Database.Column().Replace(" ", "");
}
