using StorageSystem.IntegrationTests.Base;

namespace StorageSystem.IntegrationTests.Application.UseCases.Files.GetFileDownload;

[CollectionDefinition(nameof(GetFileDownloadTestFixture))]
public class GetFileDownloadTestFixtureCollection : ICollectionFixture<GetFileDownloadTestFixture> { }

public class GetFileDownloadTestFixture : BaseFixture
{
    public string GetValidDownloadUrl() => Faker.Internet.Url();
}
