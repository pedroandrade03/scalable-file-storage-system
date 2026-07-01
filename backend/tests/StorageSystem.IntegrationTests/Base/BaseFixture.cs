using Bogus;
using Microsoft.EntityFrameworkCore;
using StorageSystem.Domain.Entities;
using StorageSystem.Infrastructure.Data.EF.Persistence.Contexts;
using StorageSystem.Infrastructure.Data.EF.Persistence.UnitOfWork;
using StorageSystem.Infrastructure.Data.EF.Repositories;

namespace StorageSystem.IntegrationTests.Base;

public abstract class BaseFixture
{
    public Faker Faker { get; }

    protected BaseFixture() => Faker = new Faker("pt_BR");

    public ApplicationDbContext CreateDbContext(bool preserveData = false)
    {
        var context = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("integration-tests-db")
                .Options
        );

        if (preserveData is false)
            context.Database.EnsureDeleted();

        return context;
    }

    public EfUnitOfWork CreateUnitOfWork(ApplicationDbContext context) => new(context);

    public FolderRepository CreateFolderRepository(ApplicationDbContext context) => new(context);

    public FileRepository CreateFileRepository(ApplicationDbContext context) => new(context);

    public UserRepository CreateUserRepository(ApplicationDbContext context) => new(context);

    public bool GetRandomBoolean() => Faker.Random.Bool();

    public string GetValidUserName() => Faker.Name.FullName();

    public string GetValidUserEmail() => Faker.Internet.Email();

    public string GetValidExternalProvider() => "keycloak";

    public string GetValidExternalSubject() => Faker.Random.Guid().ToString();

    public User GetExampleUser() => new(
        GetValidUserName(),
        GetValidUserEmail(),
        GetValidExternalProvider(),
        GetValidExternalSubject()
    );

    public List<User> GetExampleUsersList(int count = 5) =>
        Enumerable.Range(1, count).Select(_ => GetExampleUser()).ToList();

    public string GetValidFolderName() => Faker.Commerce.Department();

    public Folder GetExampleFolder(Guid? userId = null, Guid? parentFolderId = null) =>
        new(GetValidFolderName(), userId ?? Faker.Random.Guid(), parentFolderId);

    public List<Folder> GetExampleFoldersList(Guid userId, int count = 5) =>
        Enumerable.Range(1, count).Select(_ => GetExampleFolder(userId)).ToList();

    public string GetValidFileName() => Faker.System.FileName("pdf");

    public string GetValidContentType() => "application/pdf";

    public long GetValidSizeBytes() => Faker.Random.Long(1, 10_485_760);

    public FileItem GetExampleFile(Guid? userId = null, Guid? folderId = null) =>
        new(
            GetValidFileName(),
            GetValidContentType(),
            GetValidSizeBytes(),
            folderId ?? Faker.Random.Guid(),
            userId ?? Faker.Random.Guid()
        );
}
