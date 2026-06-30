using FluentAssertions;
using Moq;
using StorageSystem.Application.Exceptions;
using StorageSystem.Application.Interfaces;
using StorageSystem.Application.UseCases.Files.GetFileDownload;

namespace StorageSystem.IntegrationTests.Application.UseCases.Files.GetFileDownload;

[Collection(nameof(GetFileDownloadTestFixture))]
public class GetFileDownloadTest
{
    private readonly GetFileDownloadTestFixture _fixture;

    public GetFileDownloadTest(GetFileDownloadTestFixture fixture) => _fixture = fixture;

    [Fact(DisplayName = nameof(GetFileDownload))]
    [Trait("Integration/Application", "GetFileDownload - Use Cases")]
    public async Task GetFileDownload()
    {
        var dbContext = _fixture.CreateDbContext();
        var file = _fixture.GetExampleFile();
        await dbContext.Files.AddAsync(file);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var downloadUrl = _fixture.GetValidDownloadUrl();
        var downloadUrlProvider = new Mock<IFileDownloadUrlProvider>();
        downloadUrlProvider
            .Setup(p => p.CreateDownloadUrlAsync(file.StorageKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(downloadUrl);

        var handler = new GetFileDownloadQueryHandler(
            _fixture.CreateFileRepository(_fixture.CreateDbContext(true)),
            downloadUrlProvider.Object
        );

        var output = await handler.Handle(new GetFileDownloadQuery(file.Id), CancellationToken.None);

        output.Should().NotBeNull();
        output.FileId.Should().Be(file.Id);
        output.DownloadUrl.Should().Be(downloadUrl);
        downloadUrlProvider.Verify(
            p => p.CreateDownloadUrlAsync(file.StorageKey, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact(DisplayName = nameof(ThrowWhenFileNotFound))]
    [Trait("Integration/Application", "GetFileDownload - Use Cases")]
    public async Task ThrowWhenFileNotFound()
    {
        var dbContext = _fixture.CreateDbContext();
        await dbContext.Files.AddAsync(_fixture.GetExampleFile());
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var downloadUrlProvider = new Mock<IFileDownloadUrlProvider>();
        var handler = new GetFileDownloadQueryHandler(
            _fixture.CreateFileRepository(_fixture.CreateDbContext(true)),
            downloadUrlProvider.Object
        );
        var missingFileId = Guid.NewGuid();

        var action = () => handler.Handle(new GetFileDownloadQuery(missingFileId), CancellationToken.None);

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"File '{missingFileId}' was not found.");

        downloadUrlProvider.Verify(
            p => p.CreateDownloadUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
