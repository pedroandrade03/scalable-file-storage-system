namespace StorageSystem.Application.Exceptions;

public class ApplicationValidationException : Exception
{
    public ApplicationValidationException(string message) : base(message)
    {
        Errors = [message];
    }

    public ApplicationValidationException(IEnumerable<string> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors.ToArray();
    }

    public IReadOnlyCollection<string> Errors { get; }
}
