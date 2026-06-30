using Moq;
using StorageSystem.Domain.Repositories;
using StorageSystem.UnitTests.Common;
using DomainEntity = StorageSystem.Domain.Entities;

namespace StorageSystem.UnitTests.Application.Folders.Common;

public class FolderUseCasesBaseFixture : BaseFixture
{
    public Mock<IFolderRepository> GetFolderRepositoryMock() => new();

    public Mock<IUserRepository> GetUserRepositoryMock() => new();

    public string GetValidFolderName() => Faker.Commerce.Department();

    public Guid GetValidUserId() => Faker.Random.Guid();

    public DomainEntity.Folder GetExampleFolder(
        Guid? userId = null,
        Guid? parentFolderId = null,
        string? name = null
    ) => new(
        name ?? GetValidFolderName(),
        userId ?? GetValidUserId(),
        parentFolderId
    );
}
