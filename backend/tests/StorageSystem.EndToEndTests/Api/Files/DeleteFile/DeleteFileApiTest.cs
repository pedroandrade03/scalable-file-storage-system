using System.Net;
using FluentAssertions;
using StorageSystem.EndToEndTests.ApiModels;
using StorageSystem.EndToEndTests.Common;

namespace StorageSystem.EndToEndTests.Api.Files.DeleteFile;

[Collection(nameof(DeleteFileApiTestFixture))]
public class DeleteFileApiTest(DeleteFileApiTestFixture fixture) : IDisposable
{
    [Fact(DisplayName = nameof(DeleteFileNoContent))]
    [Trait("EndToEnd/Api", "Files/Delete - Endpoints")]
    public async Task DeleteFileNoContent()
    {
        FakeFileStorageRemover.Reset();
        var file = await fixture.CreateFileAsync();

        var (response, output) = await fixture.ApiClient
            .DeleteAsync<TestProblemDetails>($"/files/{file.Id}");

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.NoContent);
        output.Should().BeNull();
        FakeFileStorageRemover.DeletedStorageKeys.Should().ContainSingle(file.StorageKey);

        var (downloadResponse, downloadOutput) = await fixture.ApiClient
            .GetAsync<TestProblemDetails>($"/files/{file.Id}/download");

        downloadResponse!.StatusCode.Should().Be(HttpStatusCode.NotFound);
        downloadOutput.Should().NotBeNull();
        downloadOutput!.Status.Should().Be((int)HttpStatusCode.NotFound);
        downloadOutput.Detail.Should().Be($"File '{file.Id}' was not found.");
    }

    [Fact(DisplayName = nameof(ErrorWhenFileNotFound))]
    [Trait("EndToEnd/Api", "Files/Delete - Endpoints")]
    public async Task ErrorWhenFileNotFound()
    {
        FakeFileStorageRemover.Reset();
        var missingFileId = Guid.NewGuid();

        var (response, output) = await fixture.ApiClient
            .DeleteAsync<TestProblemDetails>($"/files/{missingFileId}");

        response!.StatusCode.Should().Be(HttpStatusCode.NotFound);
        output.Should().NotBeNull();
        output!.Status.Should().Be((int)HttpStatusCode.NotFound);
        output.Detail.Should().Be($"File '{missingFileId}' was not found.");
        FakeFileStorageRemover.DeletedStorageKeys.Should().BeEmpty();
    }

    public void Dispose()
    {
        FakeFileStorageRemover.Reset();
        fixture.CleanPersistence();
    }
}
