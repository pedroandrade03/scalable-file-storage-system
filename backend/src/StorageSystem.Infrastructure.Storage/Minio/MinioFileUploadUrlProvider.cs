using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using StorageSystem.Application.Interfaces;

namespace StorageSystem.Infrastructure.Storage.Minio;

public class MinioFileUploadUrlProvider(IOptions<MinioOptions> options) :
    IFileUploadUrlProvider,
    IFileDownloadUrlProvider,
    IFileStorageRemover
{
    private readonly MinioOptions _options = options.Value;

    public async Task<string> CreateUploadUrlAsync(
        string storageKey,
        string contentType,
        long sizeBytes,
        CancellationToken cancellationToken
    )
    {
        ValidateOptions();

        var internalClient = CreateClient(_options.Endpoint);
        await EnsureBucketExistsAsync(internalClient, cancellationToken);

        var publicEndpoint = string.IsNullOrWhiteSpace(_options.PublicEndpoint)
            ? _options.Endpoint
            : _options.PublicEndpoint;

        var publicClient = CreateClient(publicEndpoint);
        var args = new PresignedPutObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(storageKey)
            .WithExpiry(_options.PresignedUrlExpirySeconds);

        return await publicClient.PresignedPutObjectAsync(args);
    }

    public async Task<string> CreateDownloadUrlAsync(
        string storageKey,
        CancellationToken cancellationToken
    )
    {
        ValidateOptions();

        var internalClient = CreateClient(_options.Endpoint);
        await EnsureBucketExistsAsync(internalClient, cancellationToken);

        var publicEndpoint = string.IsNullOrWhiteSpace(_options.PublicEndpoint)
            ? _options.Endpoint
            : _options.PublicEndpoint;

        var publicClient = CreateClient(publicEndpoint);
        var args = new PresignedGetObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(storageKey)
            .WithExpiry(_options.PresignedUrlExpirySeconds);

        return await publicClient.PresignedGetObjectAsync(args);
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        ValidateOptions();

        var internalClient = CreateClient(_options.Endpoint);
        await EnsureBucketExistsAsync(internalClient, cancellationToken);

        var args = new RemoveObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(storageKey);

        await internalClient.RemoveObjectAsync(args, cancellationToken);
    }

    private async Task EnsureBucketExistsAsync(
        IMinioClient minioClient,
        CancellationToken cancellationToken
    )
    {
        var bucketExistsArgs = new BucketExistsArgs()
            .WithBucket(_options.BucketName);

        var bucketExists = await minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken);
        if (bucketExists)
        {
            return;
        }

        var makeBucketArgs = new MakeBucketArgs()
            .WithBucket(_options.BucketName);

        await minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);
    }

    private IMinioClient CreateClient(string endpoint)
        => new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(_options.AccessKey, _options.SecretKey)
            .WithSSL(_options.UseSsl)
            .Build();

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.Endpoint))
        {
            throw new InvalidOperationException("Minio endpoint was not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.AccessKey))
        {
            throw new InvalidOperationException("Minio access key was not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            throw new InvalidOperationException("Minio secret key was not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.BucketName))
        {
            throw new InvalidOperationException("Minio bucket name was not configured.");
        }
    }
}
