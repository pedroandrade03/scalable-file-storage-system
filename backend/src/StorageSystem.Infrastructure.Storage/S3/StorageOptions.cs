namespace StorageSystem.Infrastructure.Storage.S3;

public class StorageOptions
{
    public string Endpoint { get; init; } = string.Empty;
    public string PublicEndpoint { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string BucketName { get; init; } = "files";
    public string Region { get; init; } = "us-east-1";
    public bool UseSsl { get; init; }
    public int PresignedUrlExpirySeconds { get; init; } = 900;
    public long MultipartPartSizeBytes { get; init; } = 8L * 1024 * 1024;
}
