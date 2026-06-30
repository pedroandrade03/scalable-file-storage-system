using Moq;
using StorageSystem.Application.Interfaces;
using StorageSystem.Domain.Repositories;
using StorageSystem.UnitTests.Application.Folders.Common;
using DomainEntity = StorageSystem.Domain.Entities;

namespace StorageSystem.UnitTests.Application.Files.Common;

public class FileUseCasesBaseFixture : FolderUseCasesBaseFixture
{
    public Mock<IFileRepository> GetFileRepositoryMock() => new();

    public Mock<IFileUploadUrlProvider> GetUploadUrlProviderMock() => new();

    public Mock<IFileDownloadUrlProvider> GetDownloadUrlProviderMock() => new();

    public string GetValidFileName() => Faker.System.FileName("pdf");

    public string GetValidContentType() => "application/pdf";

    public long GetValidSizeBytes() => Faker.Random.Long(1, 10_485_760);

    public DomainEntity.FileItem GetExampleFile(
        Guid? userId = null,
        Guid? folderId = null,
        string? name = null,
        string? contentType = null,
        long? sizeBytes = null
    ) => new(
        name ?? GetValidFileName(),
        contentType ?? GetValidContentType(),
        sizeBytes ?? GetValidSizeBytes(),
        folderId ?? Faker.Random.Guid(),
        userId ?? GetValidUserId()
    );
}
