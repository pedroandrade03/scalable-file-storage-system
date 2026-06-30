namespace StorageSystem.EndToEndTests.Api.Folders.CreateFolder;

public static class CreateFolderApiTestDataGenerator
{
    public static IEnumerable<object[]> GetInvalidNames()
    {
        yield return new object[] { string.Empty };
        yield return new object[] { new string('a', 256) };
    }
}
