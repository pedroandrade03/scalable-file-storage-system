using FluentValidation;

namespace StorageSystem.Application.UseCases.Files.CompleteFileUpload;

public class CompleteFileUploadCommandValidator : AbstractValidator<CompleteFileUploadCommand>
{
    public CompleteFileUploadCommandValidator()
    {
        RuleFor(command => command.FileId)
            .NotEmpty();

        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.UploadId)
            .NotEmpty();

        RuleFor(command => command.Parts)
            .NotEmpty();

        RuleForEach(command => command.Parts)
            .ChildRules(part =>
            {
                part.RuleFor(item => item.PartNumber)
                    .GreaterThan(0);

                part.RuleFor(item => item.ETag)
                    .NotEmpty();
            });
    }
}
