using FluentValidation;

namespace StorageSystem.Application.UseCases.Folders.DeleteFolder;

public class DeleteFolderCommandValidator : AbstractValidator<DeleteFolderCommand>
{
    public DeleteFolderCommandValidator()
    {
        RuleFor(command => command.FolderId)
            .NotEmpty();

        RuleFor(command => command.UserId)
            .NotEmpty();
    }
}
