using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using StorageSystem.Application.Interfaces;

namespace StorageSystem.Infrastructure.Storage.S3;

public class S3FileStorageProvider(
    IAmazonS3 storageClient,
    IOptions<StorageOptions> options
) :
    IFileUploadUrlProvider,
    IFileDownloadUrlProvider,
    IFileMultipartUploadCompleter,
    IFileStorageRemover
{
    private const long MinimumMultipartPartSizeBytes = 5L * 1024 * 1024;
    private const long MaximumMultipartPartSizeBytes = 5L * 1024 * 1024 * 1024;
    private const int MaximumMultipartParts = 10_000;

    private readonly IAmazonS3 _storageClient = storageClient;
    private readonly StorageOptions _options = options.Value;

    public async Task<MultipartUploadPlan> CreateUploadUrlAsync(
        string storageKey,
        string contentType,
        long sizeBytes,
        CancellationToken cancellationToken
    )
    {
        ValidateOptions();
        ValidateUploadInput(sizeBytes);

        await EnsureBucketExistsAsync(cancellationToken);

        var multipartUpload = await _storageClient.InitiateMultipartUploadAsync(
            new InitiateMultipartUploadRequest
            {
                BucketName = _options.BucketName,
                Key = storageKey,
                ContentType = contentType
            },
            cancellationToken
        );

        var partSizeBytes = CalculatePartSizeBytes(sizeBytes);
        var totalParts = (int)DivideRoundUp(sizeBytes, partSizeBytes);
        var expiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(_options.PresignedUrlExpirySeconds);
        var publicEndpoint = string.IsNullOrWhiteSpace(_options.PublicEndpoint)
            ? _options.Endpoint
            : _options.PublicEndpoint;

        using var publicClient = CreateClient(publicEndpoint);
        var uploadUrls = Enumerable
            .Range(1, totalParts)
            .Select(partNumber => new MultipartUploadPartUrl(
                partNumber,
                CreateUploadPartUrl(
                    publicClient,
                    storageKey,
                    multipartUpload.UploadId,
                    partNumber,
                    expiresAtUtc
                )
            ))
            .ToArray();

        return new MultipartUploadPlan(
            multipartUpload.UploadId,
            partSizeBytes,
            totalParts,
            expiresAtUtc,
            uploadUrls
        );
    }

    public async Task<string> CreateDownloadUrlAsync(
        string storageKey,
        CancellationToken cancellationToken
    )
    {
        ValidateOptions();

        await EnsureBucketExistsAsync(cancellationToken);

        var publicEndpoint = string.IsNullOrWhiteSpace(_options.PublicEndpoint)
            ? _options.Endpoint
            : _options.PublicEndpoint;

        using var publicClient = CreateClient(publicEndpoint);
        return publicClient.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = storageKey,
            Verb = HttpVerb.GET,
            Protocol = _options.UseSsl ? Protocol.HTTPS : Protocol.HTTP,
            Expires = DateTime.UtcNow.AddSeconds(_options.PresignedUrlExpirySeconds)
        });
    }

    public async Task CompleteMultipartUploadAsync(
        string storageKey,
        string uploadId,
        IReadOnlyList<CompletedMultipartUploadPart> parts,
        CancellationToken cancellationToken
    )
    {
        ValidateOptions();

        await EnsureBucketExistsAsync(cancellationToken);

        var partETags = parts
            .OrderBy(part => part.PartNumber)
            .Select(part => new PartETag(part.PartNumber, part.ETag))
            .ToList();

        await _storageClient.CompleteMultipartUploadAsync(
            new CompleteMultipartUploadRequest
            {
                BucketName = _options.BucketName,
                Key = storageKey,
                UploadId = uploadId,
                PartETags = partETags
            },
            cancellationToken
        );
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        ValidateOptions();

        await EnsureBucketExistsAsync(cancellationToken);

        await _storageClient.DeleteObjectAsync(
            new DeleteObjectRequest
            {
                BucketName = _options.BucketName,
                Key = storageKey
            },
            cancellationToken
        );
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _storageClient.HeadBucketAsync(
                new HeadBucketRequest
                {
                    BucketName = _options.BucketName
                },
                cancellationToken
            );
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            await _storageClient.PutBucketAsync(
                new PutBucketRequest
                {
                    BucketName = _options.BucketName,
                    UseClientRegion = true
                },
                cancellationToken
            );
        }
    }

    private IAmazonS3 CreateClient(string endpoint)
    {
        var normalizedEndpoint = NormalizeEndpoint(endpoint);
        var credentials = new BasicAWSCredentials(_options.AccessKey, _options.SecretKey);
        var config = new AmazonS3Config
        {
            AuthenticationRegion = _options.Region,
            ServiceURL = normalizedEndpoint,
            ForcePathStyle = true,
            UseHttp = normalizedEndpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
        };

        return new AmazonS3Client(credentials, config);
    }

    private string CreateUploadPartUrl(
        IAmazonS3 publicClient,
        string storageKey,
        string uploadId,
        int partNumber,
        DateTimeOffset expiresAtUtc
    )
        => publicClient.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = storageKey,
            Verb = HttpVerb.PUT,
            Protocol = _options.UseSsl ? Protocol.HTTPS : Protocol.HTTP,
            Expires = expiresAtUtc.UtcDateTime,
            UploadId = uploadId,
            PartNumber = partNumber
        });

    private long CalculatePartSizeBytes(long sizeBytes)
    {
        var partSizeBytes = _options.MultipartPartSizeBytes;
        var totalParts = DivideRoundUp(sizeBytes, partSizeBytes);
        if (totalParts <= MaximumMultipartParts)
        {
            return partSizeBytes;
        }

        var requiredPartSize = DivideRoundUp(sizeBytes, MaximumMultipartParts);
        if (requiredPartSize > MaximumMultipartPartSizeBytes)
        {
            throw new InvalidOperationException(
                $"File size '{sizeBytes}' exceeds the multipart upload limit."
            );
        }

        return requiredPartSize;
    }

    private static long DivideRoundUp(long value, long divisor)
        => value / divisor + (value % divisor == 0 ? 0 : 1);

    private string NormalizeEndpoint(string endpoint)
    {
        var trimmedEndpoint = endpoint.Trim();
        if (trimmedEndpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || trimmedEndpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return trimmedEndpoint;
        }

        var scheme = _options.UseSsl ? "https" : "http";
        return $"{scheme}://{trimmedEndpoint}";
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.Endpoint))
        {
            throw new InvalidOperationException("Storage endpoint was not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.AccessKey))
        {
            throw new InvalidOperationException("Storage access key was not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            throw new InvalidOperationException("Storage secret key was not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.BucketName))
        {
            throw new InvalidOperationException("Storage bucket name was not configured.");
        }

        if (_options.PresignedUrlExpirySeconds <= 0)
        {
            throw new InvalidOperationException("Storage presigned URL expiry must be greater than zero.");
        }

        if (_options.MultipartPartSizeBytes < MinimumMultipartPartSizeBytes)
        {
            throw new InvalidOperationException(
                $"Storage multipart part size must be at least {MinimumMultipartPartSizeBytes} bytes."
            );
        }

        if (_options.MultipartPartSizeBytes > MaximumMultipartPartSizeBytes)
        {
            throw new InvalidOperationException(
                $"Storage multipart part size must be at most {MaximumMultipartPartSizeBytes} bytes."
            );
        }
    }

    private static void ValidateUploadInput(long sizeBytes)
    {
        if (sizeBytes <= 0)
        {
            throw new InvalidOperationException("Upload size must be greater than zero.");
        }
    }
}
