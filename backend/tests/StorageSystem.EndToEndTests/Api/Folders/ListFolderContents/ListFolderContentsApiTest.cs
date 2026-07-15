using System.Net;
using FluentAssertions;
using StorageSystem.Application.UseCases.Folders.ListFolderContents;
using StorageSystem.EndToEndTests.ApiModels;

namespace StorageSystem.EndToEndTests.Api.Folders.ListFolderContents;

[Collection(nameof(ListFolderContentsApiTestFixture))]
public class ListFolderContentsApiTest(ListFolderContentsApiTestFixture fixture) : IDisposable
{
    [Fact(DisplayName = nameof(ListRootFolderContentsOk))]
    [Trait("EndToEnd/Api", "Folders/ListContents - Endpoints")]
    public async Task ListRootFolderContentsOk()
    {
        var firstFolder = await fixture.CreateFolderAsync();
        var secondFolder = await fixture.CreateFolderAsync();

        var (response, output) = await fixture.ApiClient
            .GetAsync<TestApiResponse<ListFolderContentsOutput>>("/folders");

        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.ParentFolderId.Should().BeNull();
        output.Data.Folders.Select(folder => folder.Id).Should()
            .BeEquivalentTo([firstFolder.Id, secondFolder.Id]);
        output.Data.Files.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(ListChildFolderContentsOk))]
    [Trait("EndToEnd/Api", "Folders/ListContents - Endpoints")]
    public async Task ListChildFolderContentsOk()
    {
        var parentFolder = await fixture.CreateFolderAsync();
        var childFolder = await fixture.CreateFolderAsync(parentFolder.Id);
        var file = await fixture.CreateFileAsync(parentFolder.Id);

        var (response, output) = await fixture.ApiClient
            .GetAsync<TestApiResponse<ListFolderContentsOutput>>(
                $"/folders?parentFolderId={parentFolder.Id}"
            );

        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.ParentFolderId.Should().Be(parentFolder.Id);
        output.Data.Folders.Should().ContainSingle();
        output.Data.Folders[0].Id.Should().Be(childFolder.Id);
        output.Data.Files.Should().ContainSingle();
        output.Data.Files[0].Id.Should().Be(file.Id);
    }

    [Fact(DisplayName = nameof(ErrorWhenParentFolderNotFound))]
    [Trait("EndToEnd/Api", "Folders/ListContents - Endpoints")]
    public async Task ErrorWhenParentFolderNotFound()
    {
        var missingFolderId = Guid.NewGuid();

        var (response, output) = await fixture.ApiClient
            .GetAsync<TestProblemDetails>($"/folders?parentFolderId={missingFolderId}");

        response!.StatusCode.Should().Be(HttpStatusCode.NotFound);
        output.Should().NotBeNull();
        output!.Status.Should().Be((int)HttpStatusCode.NotFound);
        output.Detail.Should().Be($"Folder '{missingFolderId}' was not found.");
    }

    public void Dispose() => fixture.CleanPersistence();
}
