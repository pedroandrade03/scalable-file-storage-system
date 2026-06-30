using StorageSystem.Application.UseCases.Files.CreateFile;
using StorageSystem.IntegrationTests.Base;

namespace StorageSystem.IntegrationTests.Application.UseCases.Files.CreateFile;

[CollectionDefinition(nameof(CreateFileTestFixture))]
public class CreateFileTestFixtureCollection : ICollectionFixture<CreateFileTestFixture> { }

public class CreateFileTestFixture : BaseFixture
{
    public CreateFileCommand GetValidCommand(Guid userId, Guid folderId) =>
        new(GetValidFileName(), GetValidContentType(), GetValidSizeBytes(), folderId, userId);
}
