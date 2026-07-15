using Amazon.S3;
using Amazon.S3.Model;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using StorageSystem.Application.Interfaces;
using StorageSystem.Infrastructure.Storage.S3;

namespace StorageSystem.UnitTests.Infrastructure.Storage.S3;

public class S3FileStorageProviderTest
{
    [Fact(DisplayName = nameof(CreateUploadUrlAsyncReturnsMultipartPlan))]
    [Trait("Infrastructure", "S3 Storage")]
    public async Task CreateUploadUrlAsyncReturnsMultipartPlan()
    {
        var storageClient = new Mock<IAmazonS3>();
        var options = Options.Create(new StorageOptions
        {
            Endpoint = "minio:9000",
            PublicEndpoint = "localhost:9000",
            AccessKey = "access-key",
            SecretKey = "secret-key",
            BucketName = "files",
            UseSsl = false,
            PresignedUrlExpirySeconds = 900,
            MultipartPartSizeBytes = 5 * 1024 * 1024
        });

        storageClient
            .Setup(client => client.HeadBucketAsync(
                It.Is<HeadBucketRequest>(request => request.BucketName == options.Value.BucketName),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new HeadBucketResponse());

        storageClient
            .Setup(client => client.InitiateMultipartUploadAsync(
                It.Is<InitiateMultipartUploadRequest>(request =>
                    request.BucketName == options.Value.BucketName
                    && request.Key == "users/user-id/file.txt"
                    && request.ContentType == "text/plain"
                ),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new InitiateMultipartUploadResponse
            {
                UploadId = "upload-id"
            });

        var provider = new S3FileStorageProvider(storageClient.Object, options);

        var plan = await provider.CreateUploadUrlAsync(
            "users/user-id/file.txt",
            "text/plain",
            11 * 1024 * 1024,
            CancellationToken.None
        );

        plan.UploadId.Should().Be("upload-id");
        plan.PartSizeBytes.Should().Be(5 * 1024 * 1024);
        plan.TotalParts.Should().Be(3);
        plan.ExpiresAtUtc.Should().BeAfter(DateTimeOffset.UtcNow);
        plan.Parts.Select(part => part.PartNumber).Should().Equal([1, 2, 3]);
        plan.Parts.Should().OnlyContain(part => part.Url.StartsWith("http://localhost:9000/files/"));
        plan.Parts[0].Url.Should().Contain("partNumber=1");
        plan.Parts[0].Url.Should().Contain("uploadId=upload-id");
        plan.Parts[1].Url.Should().Contain("partNumber=2");
        plan.Parts[2].Url.Should().Contain("partNumber=3");
    }

    [Fact(DisplayName = nameof(CompleteMultipartUploadAsyncCompletesParts))]
    [Trait("Infrastructure", "S3 Storage")]
    public async Task CompleteMultipartUploadAsyncCompletesParts()
    {
        var storageClient = new Mock<IAmazonS3>();
        var options = Options.Create(new StorageOptions
        {
            Endpoint = "minio:9000",
            AccessKey = "access-key",
            SecretKey = "secret-key",
            BucketName = "files",
            UseSsl = false
        });
        IReadOnlyList<CompletedMultipartUploadPart> parts =
        [
            new(2, "\"etag-2\""),
            new(1, "\"etag-1\"")
        ];

        storageClient
            .Setup(client => client.HeadBucketAsync(
                It.Is<HeadBucketRequest>(request => request.BucketName == options.Value.BucketName),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new HeadBucketResponse());

        storageClient
            .Setup(client => client.CompleteMultipartUploadAsync(
                It.Is<CompleteMultipartUploadRequest>(request =>
                    request.BucketName == options.Value.BucketName
                    && request.Key == "users/user-id/file.txt"
                    && request.UploadId == "upload-id"
                    && request.PartETags.Select(part => part.PartNumber.GetValueOrDefault()).SequenceEqual(new[] { 1, 2 })
                    && request.PartETags.Select(part => part.ETag).SequenceEqual(new[] { "\"etag-1\"", "\"etag-2\"" })
                ),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new CompleteMultipartUploadResponse());

        var provider = new S3FileStorageProvider(storageClient.Object, options);

        await provider.CompleteMultipartUploadAsync(
            "users/user-id/file.txt",
            "upload-id",
            parts,
            CancellationToken.None
        );

        storageClient.Verify(
            client => client.CompleteMultipartUploadAsync(
                It.IsAny<CompleteMultipartUploadRequest>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
}
