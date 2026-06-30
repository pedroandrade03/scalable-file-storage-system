using FluentAssertions;
using StorageSystem.Domain.Exceptions;
using StorageSystem.Domain.Validation;

namespace StorageSystem.UnitTests.Domain.Validation;

[Collection(nameof(DomainValidationTestFixture))]
public class DomainValidationTest
{
    private readonly DomainValidationTestFixture _fixture;

    public DomainValidationTest(DomainValidationTestFixture fixture) => _fixture = fixture;

    [Fact(DisplayName = nameof(NotNullOk))]
    [Trait("Domain", "DomainValidation - Validation")]
    public void NotNullOk()
    {
        var value = _fixture.Faker.Commerce.ProductName();
        var fieldName = _fixture.GetValidFieldName();

        var action = () => DomainValidation.NotNull(value, fieldName);

        action.Should().NotThrow();
    }

    [Fact(DisplayName = nameof(NotNullThrowWhenNull))]
    [Trait("Domain", "DomainValidation - Validation")]
    public void NotNullThrowWhenNull()
    {
        var fieldName = _fixture.GetValidFieldName();

        var action = () => DomainValidation.NotNull(null, fieldName);

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage($"{fieldName} should not be null.");
    }

    [Theory(DisplayName = nameof(NotNullOrEmptyThrowWhenEmpty))]
    [Trait("Domain", "DomainValidation - Validation")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void NotNullOrEmptyThrowWhenEmpty(string? target)
    {
        var fieldName = _fixture.GetValidFieldName();

        var action = () => DomainValidation.NotNullOrEmpty(target, fieldName);

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage($"{fieldName} should not be empty or null.");
    }

    [Fact(DisplayName = nameof(NotNullOrEmptyOk))]
    [Trait("Domain", "DomainValidation - Validation")]
    public void NotNullOrEmptyOk()
    {
        var value = _fixture.Faker.Commerce.ProductName();
        var fieldName = _fixture.GetValidFieldName();

        var action = () => DomainValidation.NotNullOrEmpty(value, fieldName);

        action.Should().NotThrow();
    }

    [Fact(DisplayName = nameof(MinLengthThrowWhenLess))]
    [Trait("Domain", "DomainValidation - Validation")]
    public void MinLengthThrowWhenLess()
    {
        var fieldName = _fixture.GetValidFieldName();

        var action = () => DomainValidation.MinLength("ab", 3, fieldName);

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage($"{fieldName} should be at least 3 characters long.");
    }

    [Fact(DisplayName = nameof(MaxLengthThrowWhenGreater))]
    [Trait("Domain", "DomainValidation - Validation")]
    public void MaxLengthThrowWhenGreater()
    {
        var fieldName = _fixture.GetValidFieldName();

        var action = () => DomainValidation.MaxLength(new string('a', 11), 10, fieldName);

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage($"{fieldName} should be less or equal 10 characters long.");
    }

    [Theory(DisplayName = nameof(GreaterThanZeroThrowWhenInvalid))]
    [Trait("Domain", "DomainValidation - Validation")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-1024)]
    public void GreaterThanZeroThrowWhenInvalid(long target)
    {
        var fieldName = _fixture.GetValidFieldName();

        var action = () => DomainValidation.GreaterThanZero(target, fieldName);

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage($"{fieldName} should be greater than zero.");
    }

    [Fact(DisplayName = nameof(GreaterThanZeroOk))]
    [Trait("Domain", "DomainValidation - Validation")]
    public void GreaterThanZeroOk()
    {
        var fieldName = _fixture.GetValidFieldName();

        var action = () => DomainValidation.GreaterThanZero(1, fieldName);

        action.Should().NotThrow();
    }
}
