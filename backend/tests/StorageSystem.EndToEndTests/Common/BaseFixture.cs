using Bogus;
using Microsoft.EntityFrameworkCore;
using StorageSystem.Infrastructure.Data.EF.Persistence.Contexts;

namespace StorageSystem.EndToEndTests.Common;

public abstract class BaseFixture
{
    public Faker Faker { get; }
    public CustomWebApplicationFactory WebApplicationFactory { get; }
    public HttpClient HttpClient { get; }
    public ApiClient ApiClient { get; }

    protected BaseFixture()
    {
        Faker = new Faker("pt_BR");
        WebApplicationFactory = new CustomWebApplicationFactory();
        HttpClient = WebApplicationFactory.CreateClient();
        ApiClient = new ApiClient(HttpClient);
    }

    public ApplicationDbContext CreateDbContext() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(
                CustomWebApplicationFactory.DatabaseName,
                CustomWebApplicationFactory.DatabaseRoot
            )
            .Options
    );

    public void CleanPersistence()
    {
        var context = CreateDbContext();
        context.Database.EnsureDeleted();
    }
}
