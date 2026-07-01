using StorageSystem.Application.UseCases.Files.CreateFile;
using StorageSystem.Application.UseCases.Folders.CreateFolder;
using StorageSystem.EndToEndTests.ApiModels;
using StorageSystem.EndToEndTests.Common;

namespace StorageSystem.EndToEndTests.Api.Folders.DeleteFolder;

[CollectionDefinition(nameof(DeleteFolderApiTestFixture))]
public class DeleteFolderApiTestFixtureCollection : ICollectionFixture<DeleteFolderApiTestFixture> { }

public class DeleteFolderApiTestFixture : BaseFixture
{
    public async Task<CreateFolderOutput> CreateFolderAsync(Guid? parentFolderId = null)
    {
        var (_, output) = await ApiClient.PostAsync<TestApiResponse<CreateFolderOutput>>(
            "/folders",
            new { name = Faker.Commerce.Department(), parentFolderId }
        );

        return output!.Data;
    }

    public async Task<CreateFileOutput> CreateFileAsync(Guid folderId)
    {
        var (_, file) = await ApiClient.PostAsync<TestApiResponse<CreateFileOutput>>(
            "/files",
            new
            {
                name = Faker.System.FileName("pdf"),
                contentType = "application/pdf",
                sizeBytes = Faker.Random.Long(1, 1_000_000),
                folderId
            }
        );

        return file!.Data;
    }
}
