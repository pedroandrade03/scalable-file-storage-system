using DomainEntity = StorageSystem.Domain.Entities;
using StorageSystem.UnitTests.Common;

namespace StorageSystem.UnitTests.Domain.Entities.FileItem;

[CollectionDefinition(nameof(FileItemTestFixture))]
public class FileItemFixtureCollection : ICollectionFixture<FileItemTestFixture> { }

public class FileItemTestFixture : BaseFixture
{
    public string GetValidFileName() => Faker.System.FileName("pdf");

    public string GetValidContentType() => "application/pdf";

    public long GetValidSizeBytes() => Faker.Random.Long(1, 10_485_760);

    public Guid GetValidFolderId() => Faker.Random.Guid();

    public Guid GetValidUserId() => Faker.Random.Guid();

    public DomainEntity.FileItem GetValidFileItem() =>
        new(
            GetValidFileName(),
            GetValidContentType(),
            GetValidSizeBytes(),
            GetValidFolderId(),
            GetValidUserId()
        );
}
