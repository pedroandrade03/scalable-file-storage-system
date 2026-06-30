namespace StorageSystem.Domain.Validation;

public static class ValidationMessages
{
    public const string FieldNotNull = "{0} should not be null.";
    public const string FieldNotNullOrEmpty = "{0} should not be empty or null.";
    public const string FieldMinLength = "{0} should be at least {1} characters long.";
    public const string FieldMaxLength = "{0} should be less or equal {1} characters long.";
    public const string FieldGreaterThanZero = "{0} should be greater than zero.";
}
