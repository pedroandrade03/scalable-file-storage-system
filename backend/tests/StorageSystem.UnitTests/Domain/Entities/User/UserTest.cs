using FluentAssertions;
using StorageSystem.Domain.Exceptions;
using DomainEntity = StorageSystem.Domain.Entities;

namespace StorageSystem.UnitTests.Domain.Entities.User;

[Collection(nameof(UserTestFixture))]
public class UserTest
{

    private readonly UserTestFixture _userTestFixture;

    public UserTest(UserTestFixture userTestFixture) => _userTestFixture = userTestFixture;

    [Fact(DisplayName = nameof(Instantiate))]
    [Trait("Domain", "User - Aggregate")]
    public void Instantiate()
    {
        var validUser = _userTestFixture.GetValidUser();
        var dateTimeBefore = DateTime.Now;

        var user = new DomainEntity.User(
            validUser.Name,
            validUser.Email,
            validUser.ExternalProvider,
            validUser.ExternalSubject
        );
        var dateTimeAfter = DateTime.Now.AddSeconds(1);

        user.Should().NotBeNull();
        user.Name.Should().Be(validUser.Name);
        user.Email.Should().Be(validUser.Email);
        user.ExternalProvider.Should().Be(validUser.ExternalProvider);
        user.ExternalSubject.Should().Be(validUser.ExternalSubject);
        user.Id.Should().NotBe(Guid.Empty);
        user.CreatedAt.Should().NotBeSameDateAs(default);
        (user.CreatedAt >= dateTimeBefore).Should().BeTrue();
        (user.CreatedAt <= dateTimeAfter).Should().BeTrue();
    }

    [Theory(DisplayName = nameof(InstantiateErrorWhenNameIsEmpty))]
    [Trait("Domain", "User - Aggregate")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void InstantiateErrorWhenNameIsEmpty(string? name)
    {
        var validUser = _userTestFixture.GetValidUser();

        var action = () => new DomainEntity.User(
            name!,
            validUser.Email,
            validUser.ExternalProvider,
            validUser.ExternalSubject
        );

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage("Name should not be empty or null.");
    }

    [Theory(DisplayName = nameof(InstantiateErrorWhenEmailIsEmpty))]
    [Trait("Domain", "User - Aggregate")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void InstantiateErrorWhenEmailIsEmpty(string? email)
    {
        var validUser = _userTestFixture.GetValidUser();

        var action = () => new DomainEntity.User(
            validUser.Name,
            email!,
            validUser.ExternalProvider,
            validUser.ExternalSubject
        );

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage("Email should not be empty or null.");
    }

    [Theory(DisplayName = nameof(InstantiateErrorWhenExternalProviderIsEmpty))]
    [Trait("Domain", "User - Aggregate")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void InstantiateErrorWhenExternalProviderIsEmpty(string? externalProvider)
    {
        var validUser = _userTestFixture.GetValidUser();

        var action = () => new DomainEntity.User(
            validUser.Name,
            validUser.Email,
            externalProvider!,
            validUser.ExternalSubject
        );

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage("ExternalProvider should not be empty or null.");
    }

    [Theory(DisplayName = nameof(InstantiateErrorWhenExternalSubjectIsEmpty))]
    [Trait("Domain", "User - Aggregate")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void InstantiateErrorWhenExternalSubjectIsEmpty(string? externalSubject)
    {
        var validUser = _userTestFixture.GetValidUser();

        var action = () => new DomainEntity.User(
            validUser.Name,
            validUser.Email,
            validUser.ExternalProvider,
            externalSubject!
        );

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage("ExternalSubject should not be empty or null.");
    }
}
