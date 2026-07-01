using FluentAssertions;
using StorageSystem.Application.UseCases.Folders.DeleteFolder;

namespace StorageSystem.UnitTests.Application.Folders.DeleteFolder;

[Collection(nameof(DeleteFolderTestFixture))]
public class DeleteFolderCommandValidatorTest
{
    private readonly DeleteFolderTestFixture _fixture;
    private readonly DeleteFolderCommandValidator _validator = new();

    public DeleteFolderCommandValidatorTest(DeleteFolderTestFixture fixture) => _fixture = fixture;

    [Fact(DisplayName = nameof(ValidateOk))]
    [Trait("Use Cases", "DeleteFolder - Validator")]
    public void ValidateOk()
    {
        var command = _fixture.GetValidCommand();

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(ValidateErrorWhenFolderIdIsEmpty))]
    [Trait("Use Cases", "DeleteFolder - Validator")]
    public void ValidateErrorWhenFolderIdIsEmpty()
    {
        var command = _fixture.GetValidCommand(folderId: Guid.Empty);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DeleteFolderCommand.FolderId));
    }

    [Fact(DisplayName = nameof(ValidateErrorWhenUserIdIsEmpty))]
    [Trait("Use Cases", "DeleteFolder - Validator")]
    public void ValidateErrorWhenUserIdIsEmpty()
    {
        var command = _fixture.GetValidCommand(userId: Guid.Empty);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DeleteFolderCommand.UserId));
    }
}
