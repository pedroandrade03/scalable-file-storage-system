namespace StorageSystem.Application.Interfaces;

public interface IFileUploadUrlProvider
{
    Task<MultipartUploadPlan> CreateUploadUrlAsync(
        string storageKey,
        string contentType,
        long sizeBytes,
        CancellationToken cancellationToken
    );
}

public sealed record MultipartUploadPlan(
    string UploadId,
    long PartSizeBytes,
    int TotalParts,
    DateTimeOffset ExpiresAtUtc,
    IReadOnlyList<MultipartUploadPartUrl> Parts
);

public sealed record MultipartUploadPartUrl(
    int PartNumber,
    string Url
);
