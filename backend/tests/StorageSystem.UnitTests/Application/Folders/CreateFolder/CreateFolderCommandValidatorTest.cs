using FluentAssertions;
using StorageSystem.Application.UseCases.Folders.CreateFolder;

namespace StorageSystem.UnitTests.Application.Folders.CreateFolder;

[Collection(nameof(CreateFolderTestFixture))]
public class CreateFolderCommandValidatorTest
{
    private readonly CreateFolderTestFixture _fixture;
    private readonly CreateFolderCommandValidator _validator = new();

    public CreateFolderCommandValidatorTest(CreateFolderTestFixture fixture) => _fixture = fixture;

    [Fact(DisplayName = nameof(ValidateOk))]
    [Trait("Use Cases", "CreateFolder - Validator")]
    public void ValidateOk()
    {
        var command = _fixture.GetValidCommand();

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory(DisplayName = nameof(ValidateErrorWhenNameIsInvalid))]
    [Trait("Use Cases", "CreateFolder - Validator")]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateErrorWhenNameIsInvalid(string name)
    {
        var command = _fixture.GetValidCommand(name: name);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFolderCommand.Name));
    }

    [Fact(DisplayName = nameof(ValidateErrorWhenNameExceedsMaxLength))]
    [Trait("Use Cases", "CreateFolder - Validator")]
    public void ValidateErrorWhenNameExceedsMaxLength()
    {
        var command = _fixture.GetValidCommand(name: new string('a', 256));

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFolderCommand.Name));
    }

    [Fact(DisplayName = nameof(ValidateErrorWhenUserIdIsEmpty))]
    [Trait("Use Cases", "CreateFolder - Validator")]
    public void ValidateErrorWhenUserIdIsEmpty()
    {
        var command = _fixture.GetValidCommand(userId: Guid.Empty);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFolderCommand.UserId));
    }
}
