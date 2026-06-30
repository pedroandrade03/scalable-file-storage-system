using System.Net;
using FluentAssertions;
using StorageSystem.Application.UseCases.Files.GetFileDownload;
using StorageSystem.EndToEndTests.ApiModels;
using StorageSystem.EndToEndTests.Common;

namespace StorageSystem.EndToEndTests.Api.Files.GetFileDownload;

[Collection(nameof(GetFileDownloadApiTestFixture))]
public class GetFileDownloadApiTest(GetFileDownloadApiTestFixture fixture) : IDisposable
{
    [Fact(DisplayName = nameof(GetFileDownloadOk))]
    [Trait("EndToEnd/Api", "Files/Download - Endpoints")]
    public async Task GetFileDownloadOk()
    {
        var file = await fixture.CreateFileAsync();

        var (response, output) = await fixture.ApiClient
            .GetAsync<TestApiResponse<GetFileDownloadOutput>>($"/files/{file.Id}/download");

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.FileId.Should().Be(file.Id);
        output.Data.DownloadUrl.Should().StartWith(FakeFileDownloadUrlProvider.DownloadUrl);
    }

    [Fact(DisplayName = nameof(ErrorWhenFileNotFound))]
    [Trait("EndToEnd/Api", "Files/Download - Endpoints")]
    public async Task ErrorWhenFileNotFound()
    {
        var missingFileId = Guid.NewGuid();

        var (response, output) = await fixture.ApiClient
            .GetAsync<TestProblemDetails>($"/files/{missingFileId}/download");

        response!.StatusCode.Should().Be(HttpStatusCode.NotFound);
        output.Should().NotBeNull();
        output!.Status.Should().Be((int)HttpStatusCode.NotFound);
        output.Detail.Should().Be($"File '{missingFileId}' was not found.");
    }

    public void Dispose() => fixture.CleanPersistence();
}
