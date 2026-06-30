using StorageSystem.Application.UseCases.Files.GetFileDownload;
using StorageSystem.UnitTests.Application.Files.Common;

namespace StorageSystem.UnitTests.Application.Files.GetFileDownload;

[CollectionDefinition(nameof(GetFileDownloadTestFixture))]
public class GetFileDownloadTestFixtureCollection : ICollectionFixture<GetFileDownloadTestFixture> { }

public class GetFileDownloadTestFixture : FileUseCasesBaseFixture
{
    public GetFileDownloadQuery GetValidQuery(Guid? fileId = null) =>
        new(fileId ?? Faker.Random.Guid());

    public string GetValidDownloadUrl() => Faker.Internet.Url();
}
