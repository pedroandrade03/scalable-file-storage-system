using Moq;
using StorageSystem.Application.Interfaces;
using StorageSystem.Application.UseCases.Files.CompleteFileUpload;
using StorageSystem.UnitTests.Application.Files.Common;

namespace StorageSystem.UnitTests.Application.Files.CompleteFileUpload;

[CollectionDefinition(nameof(CompleteFileUploadTestFixture))]
public class CompleteFileUploadTestFixtureCollection : ICollectionFixture<CompleteFileUploadTestFixture> { }

public class CompleteFileUploadTestFixture : FileUseCasesBaseFixture
{
    public Mock<IFileMultipartUploadCompleter> GetUploadCompleterMock() => new();

    public CompleteFileUploadCommand GetValidCommand(
        Guid? fileId = null,
        Guid? userId = null,
        string? uploadId = null,
        IReadOnlyList<CompletedMultipartUploadPart>? parts = null
    ) => new(
        fileId ?? Faker.Random.Guid(),
        userId ?? GetValidUserId(),
        uploadId ?? "upload-id",
        parts ??
        [
            new CompletedMultipartUploadPart(1, "\"etag-1\""),
            new CompletedMultipartUploadPart(2, "\"etag-2\"")
        ]
    );
}
