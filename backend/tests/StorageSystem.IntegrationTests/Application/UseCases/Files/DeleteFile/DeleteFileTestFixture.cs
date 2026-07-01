using StorageSystem.Application.UseCases.Files.DeleteFile;
using StorageSystem.IntegrationTests.Base;

namespace StorageSystem.IntegrationTests.Application.UseCases.Files.DeleteFile;

[CollectionDefinition(nameof(DeleteFileTestFixture))]
public class DeleteFileTestFixtureCollection : ICollectionFixture<DeleteFileTestFixture> { }

public class DeleteFileTestFixture : BaseFixture
{
    public DeleteFileCommand GetValidCommand(Guid fileId, Guid userId) => new(fileId, userId);
}
