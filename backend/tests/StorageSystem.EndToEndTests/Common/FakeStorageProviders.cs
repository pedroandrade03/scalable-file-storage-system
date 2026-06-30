using StorageSystem.Application.Interfaces;

namespace StorageSystem.EndToEndTests.Common;

public sealed class FakeFileUploadUrlProvider : IFileUploadUrlProvider
{
    public const string UploadUrl = "https://fake-storage.local/upload";

    public Task<string> CreateUploadUrlAsync(
        string storageKey,
        string contentType,
        long sizeBytes,
        CancellationToken cancellationToken
    ) => Task.FromResult($"{UploadUrl}/{storageKey}");
}

public sealed class FakeFileDownloadUrlProvider : IFileDownloadUrlProvider
{
    public const string DownloadUrl = "https://fake-storage.local/download";

    public Task<string> CreateDownloadUrlAsync(
        string storageKey,
        CancellationToken cancellationToken
    ) => Task.FromResult($"{DownloadUrl}/{storageKey}");
}
