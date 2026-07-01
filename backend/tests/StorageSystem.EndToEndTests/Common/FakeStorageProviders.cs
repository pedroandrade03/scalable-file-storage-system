using System.Collections.Concurrent;
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

public sealed class FakeFileStorageRemover : IFileStorageRemover
{
    private static readonly ConcurrentBag<string> DeletedKeys = [];

    public static IReadOnlyCollection<string> DeletedStorageKeys => DeletedKeys.ToArray();

    public static void Reset()
    {
        while (DeletedKeys.TryTake(out _))
        {
        }
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        DeletedKeys.Add(storageKey);
        return Task.CompletedTask;
    }
}
