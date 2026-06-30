using StorageSystem.Domain.Validation;

namespace StorageSystem.IntegrationTests.Application.UseCases.Folders.CreateFolder;

public static class CreateFolderTestDataGenerator
{
    public static IEnumerable<object[]> GetInvalidNames()
    {
        yield return new object[]
        {
            string.Empty,
            string.Format(ValidationMessages.FieldNotNullOrEmpty, "Name")
        };

        yield return new object[]
        {
            "   ",
            string.Format(ValidationMessages.FieldNotNullOrEmpty, "Name")
        };

        yield return new object[]
        {
            new string('a', 256),
            string.Format(ValidationMessages.FieldMaxLength, "Name", 255)
        };
    }
}
