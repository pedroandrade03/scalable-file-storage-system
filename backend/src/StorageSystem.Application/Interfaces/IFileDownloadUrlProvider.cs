namespace StorageSystem.Application.Interfaces;

public interface IFileDownloadUrlProvider
{
    Task<string> CreateDownloadUrlAsync(
        string storageKey,
        CancellationToken cancellationToken
    );
}
