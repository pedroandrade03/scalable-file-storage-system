using FluentAssertions;
using StorageSystem.Application.UseCases.Files.CreateFile;

namespace StorageSystem.UnitTests.Application.Files.CreateFile;

[Collection(nameof(CreateFileTestFixture))]
public class CreateFileCommandValidatorTest
{
    private readonly CreateFileTestFixture _fixture;
    private readonly CreateFileCommandValidator _validator = new();

    public CreateFileCommandValidatorTest(CreateFileTestFixture fixture) => _fixture = fixture;

    [Fact(DisplayName = nameof(ValidateOk))]
    [Trait("Use Cases", "CreateFile - Validator")]
    public void ValidateOk()
    {
        var command = _fixture.GetValidCommand();

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory(DisplayName = nameof(ValidateErrorWhenNameIsInvalid))]
    [Trait("Use Cases", "CreateFile - Validator")]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateErrorWhenNameIsInvalid(string name)
    {
        var command = _fixture.GetValidCommand(name: name);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFileCommand.Name));
    }

    [Theory(DisplayName = nameof(ValidateErrorWhenContentTypeIsInvalid))]
    [Trait("Use Cases", "CreateFile - Validator")]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateErrorWhenContentTypeIsInvalid(string contentType)
    {
        var command = _fixture.GetValidCommand(contentType: contentType);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFileCommand.ContentType));
    }

    [Fact(DisplayName = nameof(ValidateErrorWhenSizeBytesIsZeroOrNegative))]
    [Trait("Use Cases", "CreateFile - Validator")]
    public void ValidateErrorWhenSizeBytesIsZeroOrNegative()
    {
        var command = _fixture.GetValidCommand(sizeBytes: 0);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFileCommand.SizeBytes));
    }

    [Fact(DisplayName = nameof(ValidateErrorWhenFolderIdIsEmpty))]
    [Trait("Use Cases", "CreateFile - Validator")]
    public void ValidateErrorWhenFolderIdIsEmpty()
    {
        var command = _fixture.GetValidCommand(folderId: Guid.Empty);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFileCommand.FolderId));
    }

    [Fact(DisplayName = nameof(ValidateErrorWhenUserIdIsEmpty))]
    [Trait("Use Cases", "CreateFile - Validator")]
    public void ValidateErrorWhenUserIdIsEmpty()
    {
        var command = _fixture.GetValidCommand(userId: Guid.Empty);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFileCommand.UserId));
    }
}
