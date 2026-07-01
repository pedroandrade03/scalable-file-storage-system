using Moq;
using StorageSystem.Application.Interfaces;
using StorageSystem.Application.UseCases.Files.DeleteFile;
using StorageSystem.UnitTests.Application.Files.Common;

namespace StorageSystem.UnitTests.Application.Files.DeleteFile;

[CollectionDefinition(nameof(DeleteFileTestFixture))]
public class DeleteFileTestFixtureCollection : ICollectionFixture<DeleteFileTestFixture> { }

public class DeleteFileTestFixture : FileUseCasesBaseFixture
{
    public Mock<IFileStorageRemover> GetStorageRemoverMock() => new();

    public DeleteFileCommand GetValidCommand(
        Guid? fileId = null,
        Guid? userId = null
    ) => new(
        fileId ?? Faker.Random.Guid(),
        userId ?? GetValidUserId()
    );
}
