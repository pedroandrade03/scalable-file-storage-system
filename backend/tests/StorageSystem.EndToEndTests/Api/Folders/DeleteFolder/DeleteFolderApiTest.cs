using System.Net;
using FluentAssertions;
using StorageSystem.Application.UseCases.Files.GetFileDownload;
using StorageSystem.EndToEndTests.ApiModels;

namespace StorageSystem.EndToEndTests.Api.Folders.DeleteFolder;

[Collection(nameof(DeleteFolderApiTestFixture))]
public class DeleteFolderApiTest(DeleteFolderApiTestFixture fixture) : IDisposable
{
    [Fact(DisplayName = nameof(DeleteFolderNoContent))]
    [Trait("EndToEnd/Api", "Folders/Delete - Endpoints")]
    public async Task DeleteFolderNoContent()
    {
        var folder = await fixture.CreateFolderAsync();

        var (response, output) = await fixture.ApiClient
            .DeleteAsync<TestProblemDetails>($"/folders/{folder.Id}");

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.NoContent);
        output.Should().BeNull();
    }

    [Fact(DisplayName = nameof(ErrorWhenFolderNotFound))]
    [Trait("EndToEnd/Api", "Folders/Delete - Endpoints")]
    public async Task ErrorWhenFolderNotFound()
    {
        var missingFolderId = Guid.NewGuid();

        var (response, output) = await fixture.ApiClient
            .DeleteAsync<TestProblemDetails>($"/folders/{missingFolderId}");

        response!.StatusCode.Should().Be(HttpStatusCode.NotFound);
        output.Should().NotBeNull();
        output!.Status.Should().Be((int)HttpStatusCode.NotFound);
        output.Detail.Should().Be($"Folder '{missingFolderId}' was not found.");
    }

    [Fact(DisplayName = nameof(ErrorWhenFolderContainsFiles))]
    [Trait("EndToEnd/Api", "Folders/Delete - Endpoints")]
    public async Task ErrorWhenFolderContainsFiles()
    {
        var folder = await fixture.CreateFolderAsync();
        var file = await fixture.CreateFileAsync(folder.Id);

        var (response, output) = await fixture.ApiClient
            .DeleteAsync<TestProblemDetails>($"/folders/{folder.Id}");

        response!.StatusCode.Should().Be(HttpStatusCode.Conflict);
        output.Should().NotBeNull();
        output!.Status.Should().Be((int)HttpStatusCode.Conflict);
        output.Detail.Should().Be($"Folder '{folder.Id}' cannot be deleted because it contains files.");

        var (downloadResponse, downloadOutput) = await fixture.ApiClient
            .GetAsync<TestApiResponse<GetFileDownloadOutput>>($"/files/{file.Id}/download");

        downloadResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
        downloadOutput.Should().NotBeNull();
        downloadOutput!.Data.FileId.Should().Be(file.Id);
    }

    [Fact(DisplayName = nameof(ErrorWhenFolderContainsSubFolders))]
    [Trait("EndToEnd/Api", "Folders/Delete - Endpoints")]
    public async Task ErrorWhenFolderContainsSubFolders()
    {
        var parentFolder = await fixture.CreateFolderAsync();
        await fixture.CreateFolderAsync(parentFolder.Id);

        var (response, output) = await fixture.ApiClient
            .DeleteAsync<TestProblemDetails>($"/folders/{parentFolder.Id}");

        response!.StatusCode.Should().Be(HttpStatusCode.Conflict);
        output.Should().NotBeNull();
        output!.Status.Should().Be((int)HttpStatusCode.Conflict);
        output.Detail.Should().Be($"Folder '{parentFolder.Id}' cannot be deleted because it contains subfolders.");
    }

    public void Dispose() => fixture.CleanPersistence();
}
