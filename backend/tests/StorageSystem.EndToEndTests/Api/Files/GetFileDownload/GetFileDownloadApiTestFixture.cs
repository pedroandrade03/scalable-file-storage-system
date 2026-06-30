using StorageSystem.Application.UseCases.Files.CreateFile;
using StorageSystem.Application.UseCases.Folders.CreateFolder;
using StorageSystem.EndToEndTests.ApiModels;
using StorageSystem.EndToEndTests.Common;

namespace StorageSystem.EndToEndTests.Api.Files.GetFileDownload;

[CollectionDefinition(nameof(GetFileDownloadApiTestFixture))]
public class GetFileDownloadApiTestFixtureCollection : ICollectionFixture<GetFileDownloadApiTestFixture> { }

public class GetFileDownloadApiTestFixture : BaseFixture
{
    public async Task<CreateFileOutput> CreateFileAsync()
    {
        var (_, folder) = await ApiClient.PostAsync<TestApiResponse<CreateFolderOutput>>(
            "/folders",
            new { name = Faker.Commerce.Department(), parentFolderId = (Guid?)null }
        );

        var (_, file) = await ApiClient.PostAsync<TestApiResponse<CreateFileOutput>>(
            "/files",
            new
            {
                name = Faker.System.FileName("pdf"),
                contentType = "application/pdf",
                sizeBytes = Faker.Random.Long(1, 1_000_000),
                folderId = folder!.Data.Id
            }
        );

        return file!.Data;
    }
}
