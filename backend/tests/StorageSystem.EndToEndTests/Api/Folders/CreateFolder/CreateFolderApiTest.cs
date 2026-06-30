using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StorageSystem.Application.UseCases.Folders.CreateFolder;
using StorageSystem.EndToEndTests.ApiModels;

namespace StorageSystem.EndToEndTests.Api.Folders.CreateFolder;

[Collection(nameof(CreateFolderApiTestFixture))]
public class CreateFolderApiTest(CreateFolderApiTestFixture fixture) : IDisposable
{
    [Fact(DisplayName = nameof(CreateFolderOk))]
    [Trait("EndToEnd/Api", "Folders/Create - Endpoints")]
    public async Task CreateFolderOk()
    {
        var name = fixture.GetValidFolderName();

        var (response, output) = await fixture.ApiClient
            .PostAsync<TestApiResponse<CreateFolderOutput>>(
                "/folders",
                new { name, parentFolderId = (Guid?)null }
            );

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.Created);
        output.Should().NotBeNull();
        output!.Data.Id.Should().NotBe(Guid.Empty);
        output.Data.Name.Should().Be(name);
        output.Data.ParentFolderId.Should().BeNull();

        var dbFolder = await fixture.CreateDbContext().Folders.FindAsync(output.Data.Id);
        dbFolder.Should().NotBeNull();
        dbFolder!.Name.Should().Be(name);
        dbFolder.UserId.Should().Be(output.Data.UserId);
    }

    [Fact(DisplayName = nameof(CreateSubFolderOk))]
    [Trait("EndToEnd/Api", "Folders/Create - Endpoints")]
    public async Task CreateSubFolderOk()
    {
        var (_, parent) = await fixture.ApiClient
            .PostAsync<TestApiResponse<CreateFolderOutput>>(
                "/folders",
                new { name = fixture.GetValidFolderName(), parentFolderId = (Guid?)null }
            );

        var (response, output) = await fixture.ApiClient
            .PostAsync<TestApiResponse<CreateFolderOutput>>(
                "/folders",
                new { name = fixture.GetValidFolderName(), parentFolderId = parent!.Data.Id }
            );

        response!.StatusCode.Should().Be(HttpStatusCode.Created);
        output!.Data.ParentFolderId.Should().Be(parent.Data.Id);
    }

    [Fact(DisplayName = nameof(ErrorWhenFolderNameAlreadyExists))]
    [Trait("EndToEnd/Api", "Folders/Create - Endpoints")]
    public async Task ErrorWhenFolderNameAlreadyExists()
    {
        var name = fixture.GetValidFolderName();
        await fixture.ApiClient.PostAsync<TestApiResponse<CreateFolderOutput>>(
            "/folders",
            new { name, parentFolderId = (Guid?)null }
        );

        var (response, output) = await fixture.ApiClient.PostAsync<TestProblemDetails>(
            "/folders",
            new { name, parentFolderId = (Guid?)null }
        );

        response!.StatusCode.Should().Be(HttpStatusCode.Conflict);
        output.Should().NotBeNull();
        output!.Status.Should().Be((int)HttpStatusCode.Conflict);
        output.Detail.Should().Be($"Folder '{name}' already exists in this location.");
    }

    [Theory(DisplayName = nameof(ErrorWhenNameIsInvalid))]
    [Trait("EndToEnd/Api", "Folders/Create - Endpoints")]
    [MemberData(
        nameof(CreateFolderApiTestDataGenerator.GetInvalidNames),
        MemberType = typeof(CreateFolderApiTestDataGenerator)
    )]
    public async Task ErrorWhenNameIsInvalid(string invalidName)
    {
        var (response, output) = await fixture.ApiClient.PostAsync<TestProblemDetails>(
            "/folders",
            new { name = invalidName, parentFolderId = (Guid?)null }
        );

        response!.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        output.Should().NotBeNull();
        output!.Status.Should().Be((int)HttpStatusCode.BadRequest);
    }

    public void Dispose() => fixture.CleanPersistence();
}
