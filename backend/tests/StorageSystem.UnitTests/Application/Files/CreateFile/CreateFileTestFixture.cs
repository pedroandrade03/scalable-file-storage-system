using StorageSystem.Application.UseCases.Files.CreateFile;
using StorageSystem.UnitTests.Application.Files.Common;

namespace StorageSystem.UnitTests.Application.Files.CreateFile;

[CollectionDefinition(nameof(CreateFileTestFixture))]
public class CreateFileTestFixtureCollection : ICollectionFixture<CreateFileTestFixture> { }

public class CreateFileTestFixture : FileUseCasesBaseFixture
{
    public CreateFileCommand GetValidCommand(
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
