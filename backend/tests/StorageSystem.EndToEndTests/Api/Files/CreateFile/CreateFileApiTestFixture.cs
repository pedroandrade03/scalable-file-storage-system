using StorageSystem.Application.UseCases.Folders.CreateFolder;
using StorageSystem.EndToEndTests.ApiModels;
using StorageSystem.EndToEndTests.Common;

namespace StorageSystem.EndToEndTests.Api.Files.CreateFile;

[CollectionDefinition(nameof(CreateFileApiTestFixture))]
public class CreateFileApiTestFixtureCollection : ICollectionFixture<CreateFileApiTestFixture> { }

public class CreateFileApiTestFixture : BaseFixture
{
    public string GetValidFileName() => Faker.System.FileName("pdf");

    public string GetValidContentType() => "application/pdf";

    public long GetValidSizeBytes() => Faker.Random.Long(1, 1_000_000);

    public async Task<Guid> CreateFolderAsync()
    {
        var (_, output) = await ApiClient.PostAsync<TestApiResponse<CreateFolderOutput>>(
            "/folders",
            new { name = Faker.Commerce.Department(), parentFolderId = (Guid?)null }
        );

        return output!.Data.Id;
    }
}
