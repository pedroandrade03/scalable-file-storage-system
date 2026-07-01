namespace StorageSystem.Infrastructure.Storage.Minio;

public class MinioOptions
{
    public string Endpoint { get; init; } = string.Empty;
    public string PublicEndpoint { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string BucketName { get; init; } = "files";
    public bool UseSsl { get; init; }
    public int PresignedUrlExpirySeconds { get; init; } = 900;
}
