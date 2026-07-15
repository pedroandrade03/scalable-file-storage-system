using StorageSystem.Application.UseCases.Files.CreateFile;
using StorageSystem.Application.UseCases.Folders.CreateFolder;
using StorageSystem.EndToEndTests.ApiModels;
using StorageSystem.EndToEndTests.Common;

namespace StorageSystem.EndToEndTests.Api.Folders.ListFolderContents;

[CollectionDefinition(nameof(ListFolderContentsApiTestFixture))]
public class ListFolderContentsApiTestFixtureCollection : ICollectionFixture<ListFolderContentsApiTestFixture> { }

public class ListFolderContentsApiTestFixture : BaseFixture
{
    public string GetValidFolderName() => Faker.Commerce.Department();

    public string GetValidFileName() => Faker.System.FileName("pdf");

    public async Task<CreateFolderOutput> CreateFolderAsync(Guid? parentFolderId = null)
    {
        var (_, output) = await ApiClient.PostAsync<TestApiResponse<CreateFolderOutput>>(
            "/folders",
            new { name = GetValidFolderName(), parentFolderId }
        );

        return output!.Data;
    }

    public async Task<CreateFileOutput> CreateFileAsync(Guid folderId)
    {
        var (_, output) = await ApiClient.PostAsync<TestApiResponse<CreateFileOutput>>(
            "/files",
            new
            {
                name = GetValidFileName(),
                contentType = "application/pdf",
                sizeBytes = 128,
                folderId
            }
        );

        return output!.Data;
    }
}
