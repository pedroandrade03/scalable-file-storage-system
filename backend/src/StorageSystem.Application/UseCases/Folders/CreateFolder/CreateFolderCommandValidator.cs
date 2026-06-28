using FluentValidation;

namespace StorageSystem.Application.UseCases.Folders.CreateFolder;

public class CreateFolderCommandValidator : AbstractValidator<CreateFolderCommand>
{
    public CreateFolderCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(command => command.UserId)
            .NotEmpty();
    }
}
