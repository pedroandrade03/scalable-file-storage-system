using FluentAssertions;
using StorageSystem.Application.UseCases.Files.DeleteFile;

namespace StorageSystem.UnitTests.Application.Files.DeleteFile;

[Collection(nameof(DeleteFileTestFixture))]
public class DeleteFileCommandValidatorTest
{
    private readonly DeleteFileTestFixture _fixture;
    private readonly DeleteFileCommandValidator _validator = new();

    public DeleteFileCommandValidatorTest(DeleteFileTestFixture fixture) => _fixture = fixture;

    [Fact(DisplayName = nameof(ValidateOk))]
    [Trait("Use Cases", "DeleteFile - Validator")]
    public void ValidateOk()
    {
        var command = _fixture.GetValidCommand();

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(ValidateErrorWhenFileIdIsEmpty))]
    [Trait("Use Cases", "DeleteFile - Validator")]
    public void ValidateErrorWhenFileIdIsEmpty()
    {
        var command = _fixture.GetValidCommand(fileId: Guid.Empty);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DeleteFileCommand.FileId));
    }

    [Fact(DisplayName = nameof(ValidateErrorWhenUserIdIsEmpty))]
    [Trait("Use Cases", "DeleteFile - Validator")]
    public void ValidateErrorWhenUserIdIsEmpty()
    {
        var command = _fixture.GetValidCommand(userId: Guid.Empty);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DeleteFileCommand.UserId));
    }
}
