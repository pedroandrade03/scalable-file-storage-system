using StorageSystem.Application.UseCases.Files.CreateFile;
using StorageSystem.Application.UseCases.Folders.CreateFolder;
using StorageSystem.EndToEndTests.ApiModels;
using StorageSystem.EndToEndTests.Common;

namespace StorageSystem.EndToEndTests.Api.Files.CompleteFileUpload;

[CollectionDefinition(nameof(CompleteFileUploadApiTestFixture))]
public class CompleteFileUploadApiTestFixtureCollection : ICollectionFixture<CompleteFileUploadApiTestFixture> { }

public class CompleteFileUploadApiTestFixture : BaseFixture
{
    public string GetValidFileName() => Faker.System.FileName("pdf");

    public string GetValidContentType() => "application/pdf";

    public long GetValidSizeBytes() => Faker.Random.Long(1, 1_000_000);

    public async Task<CreateFileOutput> CreateFileAsync()
    {
        var folderId = await CreateFolderAsync();
        var (_, output) = await ApiClient.PostAsync<TestApiResponse<CreateFileOutput>>(
            "/files",
            new
            {
                name = GetValidFileName(),
                contentType = GetValidContentType(),
                sizeBytes = GetValidSizeBytes(),
                folderId
            }
        );

        return output!.Data;
    }

    private async Task<Guid> CreateFolderAsync()
    {
        var (_, output) = await ApiClient.PostAsync<TestApiResponse<CreateFolderOutput>>(
            "/folders",
            new { name = Faker.Commerce.Department(), parentFolderId = (Guid?)null }
        );

        return output!.Data.Id;
    }
}
