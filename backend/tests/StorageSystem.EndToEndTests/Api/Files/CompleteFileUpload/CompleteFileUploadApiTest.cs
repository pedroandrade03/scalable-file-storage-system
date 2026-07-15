using System.Net;
using FluentAssertions;
using StorageSystem.Application.UseCases.Files.CompleteFileUpload;
using StorageSystem.Application.UseCases.Files.CreateFile;
using StorageSystem.Domain.Enums;
using StorageSystem.EndToEndTests.ApiModels;
using StorageSystem.EndToEndTests.Common;

namespace StorageSystem.EndToEndTests.Api.Files.CompleteFileUpload;

[Collection(nameof(CompleteFileUploadApiTestFixture))]
public class CompleteFileUploadApiTest(CompleteFileUploadApiTestFixture fixture) : IDisposable
{
    [Fact(DisplayName = nameof(CompleteFileUploadOk))]
    [Trait("EndToEnd/Api", "Files/CompleteUpload - Endpoints")]
    public async Task CompleteFileUploadOk()
    {
        FakeFileMultipartUploadCompleter.Reset();
        var file = await fixture.CreateFileAsync();
        var parts = file.Upload.Parts
            .Select(part => new { part.PartNumber, eTag = $"\"etag-{part.PartNumber}\"" })
            .ToArray();

        var (response, output) = await fixture.ApiClient
            .PostAsync<TestApiResponse<CompleteFileUploadOutput>>(
                $"/files/{file.Id}/complete-upload",
                new
                {
                    file.Upload.UploadId,
                    parts
                }
            );

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Id.Should().Be(file.Id);
        output.Data.Status.Should().Be(FileStatus.Available);

        var completedUpload = FakeFileMultipartUploadCompleter.Uploads.Should().ContainSingle().Subject;
        completedUpload.StorageKey.Should().Be(file.StorageKey);
        completedUpload.UploadId.Should().Be(file.Upload.UploadId);
        completedUpload.Parts.Select(part => part.PartNumber).Should().Equal(parts.Select(part => part.PartNumber));

        var dbFile = await fixture.CreateDbContext().Files.FindAsync(file.Id);
        dbFile.Should().NotBeNull();
        dbFile!.Status.Should().Be(FileStatus.Available);
    }

    [Fact(DisplayName = nameof(ErrorWhenFileNotFound))]
    [Trait("EndToEnd/Api", "Files/CompleteUpload - Endpoints")]
    public async Task ErrorWhenFileNotFound()
    {
        FakeFileMultipartUploadCompleter.Reset();
        var missingFileId = Guid.NewGuid();

        var (response, output) = await fixture.ApiClient.PostAsync<TestProblemDetails>(
            $"/files/{missingFileId}/complete-upload",
            new
            {
                uploadId = "upload-id",
                parts = new[]
                {
                    new { partNumber = 1, eTag = "\"etag-1\"" }
                }
            }
        );

        response!.StatusCode.Should().Be(HttpStatusCode.NotFound);
        output.Should().NotBeNull();
        output!.Status.Should().Be((int)HttpStatusCode.NotFound);
        output.Detail.Should().Be($"File '{missingFileId}' was not found.");
        FakeFileMultipartUploadCompleter.Uploads.Should().BeEmpty();
    }

    public void Dispose() => fixture.CleanPersistence();
}
