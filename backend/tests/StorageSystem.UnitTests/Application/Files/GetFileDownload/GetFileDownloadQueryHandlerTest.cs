using FluentAssertions;
using Moq;
using StorageSystem.Application.Exceptions;
using StorageSystem.Application.UseCases.Files.GetFileDownload;
using DomainEntity = StorageSystem.Domain.Entities;

namespace StorageSystem.UnitTests.Application.Files.GetFileDownload;

[Collection(nameof(GetFileDownloadTestFixture))]
public class GetFileDownloadQueryHandlerTest
{
    private readonly GetFileDownloadTestFixture _fixture;

    public GetFileDownloadQueryHandlerTest(GetFileDownloadTestFixture fixture) => _fixture = fixture;

    [Fact(DisplayName = nameof(GetFileDownload))]
    [Trait("Use Cases", "GetFileDownload - Handler")]
    public async Task GetFileDownload()
    {
        var fileRepository = _fixture.GetFileRepositoryMock();
        var downloadUrlProvider = _fixture.GetDownloadUrlProviderMock();
        var file = _fixture.GetExampleFile();
        var query = _fixture.GetValidQuery(file.Id);
        var downloadUrl = _fixture.GetValidDownloadUrl();

        fileRepository
            .Setup(r => r.GetByIdAsync(file.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(file);

        downloadUrlProvider
            .Setup(p => p.CreateDownloadUrlAsync(file.StorageKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(downloadUrl);

        var handler = new GetFileDownloadQueryHandler(
            fileRepository.Object,
            downloadUrlProvider.Object
        );

        var output = await handler.Handle(query, CancellationToken.None);

        output.Should().NotBeNull();
        output.FileId.Should().Be(file.Id);
        output.DownloadUrl.Should().Be(downloadUrl);

        fileRepository.Verify(
            r => r.GetByIdAsync(file.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        downloadUrlProvider.Verify(
            p => p.CreateDownloadUrlAsync(file.StorageKey, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact(DisplayName = nameof(ThrowWhenFileNotFound))]
    [Trait("Use Cases", "GetFileDownload - Handler")]
    public async Task ThrowWhenFileNotFound()
    {
        var fileRepository = _fixture.GetFileRepositoryMock();
        var downloadUrlProvider = _fixture.GetDownloadUrlProviderMock();
        var query = _fixture.GetValidQuery();

        fileRepository
            .Setup(r => r.GetByIdAsync(query.FileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainEntity.FileItem?)null);

        var handler = new GetFileDownloadQueryHandler(
            fileRepository.Object,
            downloadUrlProvider.Object
        );

        var action = () => handler.Handle(query, CancellationToken.None);

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"File '{query.FileId}' was not found.");

        downloadUrlProvider.Verify(
            p => p.CreateDownloadUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
