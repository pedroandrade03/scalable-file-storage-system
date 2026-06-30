using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StorageSystem.Application.UseCases.Files.CreateFile;
using StorageSystem.EndToEndTests.ApiModels;
using StorageSystem.EndToEndTests.Common;

namespace StorageSystem.EndToEndTests.Api.Files.CreateFile;

[Collection(nameof(CreateFileApiTestFixture))]
public class CreateFileApiTest(CreateFileApiTestFixture fixture) : IDisposable
{
    [Fact(DisplayName = nameof(CreateFileOk))]
    [Trait("EndToEnd/Api", "Files/Create - Endpoints")]
    public async Task CreateFileOk()
    {
        var folderId = await fixture.CreateFolderAsync();
        var name = fixture.GetValidFileName();
        var contentType = fixture.GetValidContentType();
        var sizeBytes = fixture.GetValidSizeBytes();

        var (response, output) = await fixture.ApiClient
            .PostAsync<TestApiResponse<CreateFileOutput>>(
                "/files",
                new { name, contentType, sizeBytes, folderId }
            );

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.Created);
        output.Should().NotBeNull();
        output!.Data.Id.Should().NotBe(Guid.Empty);
        output.Data.Name.Should().Be(name);
        output.Data.ContentType.Should().Be(contentType);
        output.Data.SizeBytes.Should().Be(sizeBytes);
        output.Data.FolderId.Should().Be(folderId);
        output.Data.StorageKey.Should().NotBeNullOrWhiteSpace();
        output.Data.UploadUrl.Should().StartWith(FakeFileUploadUrlProvider.UploadUrl);

        var dbFile = await fixture.CreateDbContext().Files.FindAsync(output.Data.Id);
        dbFile.Should().NotBeNull();
        dbFile!.Name.Should().Be(name);
        dbFile.StorageKey.Should().Be(output.Data.StorageKey);
    }

    [Fact(DisplayName = nameof(ErrorWhenFolderNotFound))]
    [Trait("EndToEnd/Api", "Files/Create - Endpoints")]
    public async Task ErrorWhenFolderNotFound()
    {
        var missingFolderId = Guid.NewGuid();

        var (response, output) = await fixture.ApiClient.PostAsync<TestProblemDetails>(
            "/files",
            new
            {
                name = fixture.GetValidFileName(),
                contentType = fixture.GetValidContentType(),
                sizeBytes = fixture.GetValidSizeBytes(),
                folderId = missingFolderId
            }
        );

        response!.StatusCode.Should().Be(HttpStatusCode.NotFound);
        output.Should().NotBeNull();
        output!.Status.Should().Be((int)HttpStatusCode.NotFound);
        output.Detail.Should().Be($"Folder '{missingFolderId}' was not found.");
    }

    [Fact(DisplayName = nameof(ErrorWhenSizeIsInvalid))]
    [Trait("EndToEnd/Api", "Files/Create - Endpoints")]
    public async Task ErrorWhenSizeIsInvalid()
    {
        var folderId = await fixture.CreateFolderAsync();

        var (response, output) = await fixture.ApiClient.PostAsync<TestProblemDetails>(
            "/files",
            new
            {
                name = fixture.GetValidFileName(),
                contentType = fixture.GetValidContentType(),
                sizeBytes = 0L,
                folderId
            }
        );

        response!.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        output.Should().NotBeNull();
        output!.Status.Should().Be((int)HttpStatusCode.BadRequest);
    }

    public void Dispose() => fixture.CleanPersistence();
}
