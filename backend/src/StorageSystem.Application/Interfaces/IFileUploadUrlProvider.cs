namespace StorageSystem.Application.Interfaces;

public interface IFileUploadUrlProvider
{
    Task<string> CreateUploadUrlAsync(
        string storageKey,
        string contentType,
        long sizeBytes,
        CancellationToken cancellationToken
    );
}
