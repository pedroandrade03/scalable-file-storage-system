using FluentValidation;

namespace StorageSystem.Application.UseCases.Files.DeleteFile;

public class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileCommandValidator()
    {
        RuleFor(command => command.FileId)
            .NotEmpty();

        RuleFor(command => command.UserId)
            .NotEmpty();
    }
}
