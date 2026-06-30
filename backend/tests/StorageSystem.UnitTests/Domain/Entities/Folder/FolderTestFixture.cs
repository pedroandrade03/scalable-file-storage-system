using DomainEntity = StorageSystem.Domain.Entities;
using StorageSystem.UnitTests.Common;

namespace StorageSystem.UnitTests.Domain.Entities.Folder;

[CollectionDefinition(nameof(FolderTestFixture))]
public class FolderFixtureCollection : ICollectionFixture<FolderTestFixture> { }

public class FolderTestFixture : BaseFixture
{
    public string GetValidFolderName() => Faker.Commerce.Department();

    public Guid GetValidUserId() => Faker.Random.Guid();

    public Guid GetValidParentFolderId() => Faker.Random.Guid();

    public DomainEntity.Folder GetValidFolder() =>
        new(GetValidFolderName(), GetValidUserId(), GetValidParentFolderId());
}
