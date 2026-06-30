using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UnitOfWorkInfra = StorageSystem.Infrastructure.Persistence.UnitOfWork;

namespace StorageSystem.IntegrationTests.Infrastructure.Persistence.UnitOfWork;

[Collection(nameof(UnitOfWorkTestFixture))]
public class UnitOfWorkTest
{
    private readonly UnitOfWorkTestFixture _fixture;

    public UnitOfWorkTest(UnitOfWorkTestFixture fixture) => _fixture = fixture;

    [Fact(DisplayName = nameof(CommitPersistsTrackedChanges))]
    [Trait("Integration/Infrastructure", "UnitOfWork - Persistence")]
    public async Task CommitPersistsTrackedChanges()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleUsers = _fixture.GetExampleUsersList();
        await dbContext.Users.AddRangeAsync(exampleUsers);
        var unitOfWork = new UnitOfWorkInfra(dbContext);

        await unitOfWork.CommitAsync(CancellationToken.None);

        var assertDbContext = _fixture.CreateDbContext(true);
        var savedUsers = assertDbContext.Users.AsNoTracking().ToList();
        savedUsers.Should().HaveCount(exampleUsers.Count);
    }
}
