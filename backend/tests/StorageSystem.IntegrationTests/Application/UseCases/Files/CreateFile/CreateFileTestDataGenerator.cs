using StorageSystem.Domain.Validation;

namespace StorageSystem.IntegrationTests.Application.UseCases.Files.CreateFile;

public static class CreateFileTestDataGenerator
{
    public static IEnumerable<object[]> GetInvalidInputs()
    {
        const string validName = "valid-file.pdf";
        const string validContentType = "application/pdf";
        const long validSize = 1024;

        yield return
        [
            string.Empty,
            validContentType,
            validSize,
            string.Format(ValidationMessages.FieldNotNullOrEmpty, "Name")
        ];

        yield return
        [
            new string('a', 256),
            validContentType,
            validSize,
            string.Format(ValidationMessages.FieldMaxLength, "Name", 255)
        ];

        yield return
        [
            validName,
            string.Empty,
            validSize,
            string.Format(ValidationMessages.FieldNotNullOrEmpty, "ContentType")
        ];

        yield return
        [
            validName,
            new string('a', 151),
            validSize,
            string.Format(ValidationMessages.FieldMaxLength, "ContentType", 150)
        ];

        yield return
        [
            validName,
            validContentType,
            0L,
            string.Format(ValidationMessages.FieldGreaterThanZero, "SizeBytes")
        ];
    }
}
