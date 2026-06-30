using StorageSystem.Domain.Exceptions;

namespace StorageSystem.Domain.Validation;

public static class DomainValidation
{
    public static void NotNull(object? target, string fieldName)
    {
        if (target is null)
            throw new EntityValidationException(
                string.Format(ValidationMessages.FieldNotNull, fieldName)
            );
    }

    public static void NotNullOrEmpty(string? target, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(target))
            throw new EntityValidationException(
                string.Format(ValidationMessages.FieldNotNullOrEmpty, fieldName)
            );
    }

    public static void MinLength(string target, int minLength, string fieldName)
    {
        if (target.Length < minLength)
            throw new EntityValidationException(
                string.Format(ValidationMessages.FieldMinLength, fieldName, minLength)
            );
    }

    public static void MaxLength(string target, int maxLength, string fieldName)
    {
        if (target.Length > maxLength)
            throw new EntityValidationException(
                string.Format(ValidationMessages.FieldMaxLength, fieldName, maxLength)
            );
    }

    public static void GreaterThanZero(long target, string fieldName)
    {
        if (target <= 0)
            throw new EntityValidationException(
                string.Format(ValidationMessages.FieldGreaterThanZero, fieldName)
            );
    }
}
