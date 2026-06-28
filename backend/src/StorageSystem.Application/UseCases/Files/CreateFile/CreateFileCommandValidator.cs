using FluentValidation;

namespace StorageSystem.Application.UseCases.Files.CreateFile;

public class CreateFileCommandValidator : AbstractValidator<CreateFileCommand>
{
    public CreateFileCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(command => command.ContentType)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(command => command.SizeBytes)
            .GreaterThan(0);

        RuleFor(command => command.FolderId)
            .NotEmpty();

        RuleFor(command => command.UserId)
            .NotEmpty();
    }
}
