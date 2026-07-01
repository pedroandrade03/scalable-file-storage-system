using FluentAssertions;
using Repository = StorageSystem.Infrastructure.Data.EF.Repositories;

namespace StorageSystem.IntegrationTests.Infrastructure.Repositories.UserRepository;

[Collection(nameof(UserRepositoryTestFixture))]
public class UserRepositoryTest
{
    private readonly UserRepositoryTestFixture _fixture;

    public UserRepositoryTest(UserRepositoryTestFixture fixture) => _fixture = fixture;

    [Fact(DisplayName = nameof(Insert))]
    [Trait("Integration/Infrastructure", "UserRepository - Repositories")]
    public async Task Insert()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleUser = _fixture.GetExampleUser();
        var repository = new Repository.UserRepository(dbContext);

        await repository.InsertAsync(exampleUser, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var dbUser = await _fixture.CreateDbContext(true)
            .Users.FindAsync(exampleUser.Id);

        dbUser.Should().NotBeNull();
        dbUser!.Name.Should().Be(exampleUser.Name);
        dbUser.Email.Should().Be(exampleUser.Email);
        dbUser.ExternalProvider.Should().Be(exampleUser.ExternalProvider);
        dbUser.ExternalSubject.Should().Be(exampleUser.ExternalSubject);
    }

    [Fact(DisplayName = nameof(ExistsByIdReturnsTrueWhenPresent))]
    [Trait("Integration/Infrastructure", "UserRepository - Repositories")]
    public async Task ExistsByIdReturnsTrueWhenPresent()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleUser = _fixture.GetExampleUser();
        await dbContext.Users.AddAsync(exampleUser);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.UserRepository(_fixture.CreateDbContext(true));

        var exists = await repository.ExistsAsync(exampleUser.Id, CancellationToken.None);

        exists.Should().BeTrue();
    }

    [Fact(DisplayName = nameof(ExistsByIdReturnsFalseWhenAbsent))]
    [Trait("Integration/Infrastructure", "UserRepository - Repositories")]
    public async Task ExistsByIdReturnsFalseWhenAbsent()
    {
        var dbContext = _fixture.CreateDbContext();
        await dbContext.Users.AddRangeAsync(_fixture.GetExampleUsersList());
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.UserRepository(_fixture.CreateDbContext(true));

        var exists = await repository.ExistsAsync(Guid.NewGuid(), CancellationToken.None);

        exists.Should().BeFalse();
    }

    [Fact(DisplayName = nameof(GetByEmail))]
    [Trait("Integration/Infrastructure", "UserRepository - Repositories")]
    public async Task GetByEmail()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleUser = _fixture.GetExampleUser();
        var usersList = _fixture.GetExampleUsersList();
        usersList.Add(exampleUser);
        await dbContext.Users.AddRangeAsync(usersList);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.UserRepository(_fixture.CreateDbContext(true));

        var dbUser = await repository.GetByEmailAsync(exampleUser.Email, CancellationToken.None);

        dbUser.Should().NotBeNull();
        dbUser!.Id.Should().Be(exampleUser.Id);
        dbUser.Email.Should().Be(exampleUser.Email);
    }

    [Fact(DisplayName = nameof(GetByEmailReturnsNullWhenNotFound))]
    [Trait("Integration/Infrastructure", "UserRepository - Repositories")]
    public async Task GetByEmailReturnsNullWhenNotFound()
    {
        var dbContext = _fixture.CreateDbContext();
        await dbContext.Users.AddRangeAsync(_fixture.GetExampleUsersList());
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.UserRepository(_fixture.CreateDbContext(true));

        var dbUser = await repository.GetByEmailAsync("missing@example.com", CancellationToken.None);

        dbUser.Should().BeNull();
    }

    [Fact(DisplayName = nameof(GetByExternalIdentity))]
    [Trait("Integration/Infrastructure", "UserRepository - Repositories")]
    public async Task GetByExternalIdentity()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleUser = _fixture.GetExampleUser();
        var usersList = _fixture.GetExampleUsersList();
        usersList.Add(exampleUser);
        await dbContext.Users.AddRangeAsync(usersList);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.UserRepository(_fixture.CreateDbContext(true));

        var dbUser = await repository.GetByExternalIdentityAsync(
            exampleUser.ExternalProvider,
            exampleUser.ExternalSubject,
            CancellationToken.None
        );

        dbUser.Should().NotBeNull();
        dbUser!.Id.Should().Be(exampleUser.Id);
        dbUser.ExternalProvider.Should().Be(exampleUser.ExternalProvider);
        dbUser.ExternalSubject.Should().Be(exampleUser.ExternalSubject);
    }

    [Fact(DisplayName = nameof(GetByExternalIdentityReturnsNullWhenNotFound))]
    [Trait("Integration/Infrastructure", "UserRepository - Repositories")]
    public async Task GetByExternalIdentityReturnsNullWhenNotFound()
    {
        var dbContext = _fixture.CreateDbContext();
        await dbContext.Users.AddRangeAsync(_fixture.GetExampleUsersList());
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new Repository.UserRepository(_fixture.CreateDbContext(true));

        var dbUser = await repository.GetByExternalIdentityAsync(
            "keycloak",
            "missing-subject",
            CancellationToken.None
        );

        dbUser.Should().BeNull();
    }
}
