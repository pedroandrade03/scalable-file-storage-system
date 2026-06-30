using StorageSystem.EndToEndTests.Common;

namespace StorageSystem.EndToEndTests.Api.Folders.CreateFolder;

[CollectionDefinition(nameof(CreateFolderApiTestFixture))]
public class CreateFolderApiTestFixtureCollection : ICollectionFixture<CreateFolderApiTestFixture> { }

public class CreateFolderApiTestFixture : BaseFixture
{
    public string GetValidFolderName() => Faker.Commerce.Department();
}
