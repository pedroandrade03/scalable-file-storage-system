using System.Collections.Concurrent;
using StorageSystem.Application.Interfaces;

namespace StorageSystem.EndToEndTests.Common;

public sealed class FakeFileUploadUrlProvider : IFileUploadUrlProvider
{
    public const string UploadUrl = "https://fake-storage.local/upload";
    public const string UploadId = "fake-upload-id";
    public const long PartSizeBytes = 5 * 1024 * 1024;

    public Task<MultipartUploadPlan> CreateUploadUrlAsync(
        string storageKey,
        string contentType,
        long sizeBytes,
        CancellationToken cancellationToken
    )
    {
        var totalParts = (int)Math.Ceiling((double)sizeBytes / PartSizeBytes);
        var parts = Enumerable
            .Range(1, totalParts)
            .Select(partNumber => new MultipartUploadPartUrl(
                partNumber,
                $"{UploadUrl}/{storageKey}?partNumber={partNumber}&uploadId={UploadId}"
            ))
            .ToArray();

        return Task.FromResult(new MultipartUploadPlan(
            UploadId,
            PartSizeBytes,
            totalParts,
            DateTimeOffset.UtcNow.AddMinutes(15),
            parts
        ));
    }
}

public sealed class FakeFileDownloadUrlProvider : IFileDownloadUrlProvider
{
    public const string DownloadUrl = "https://fake-storage.local/download";

    public Task<string> CreateDownloadUrlAsync(
        string storageKey,
        CancellationToken cancellationToken
    ) => Task.FromResult($"{DownloadUrl}/{storageKey}");
}

public sealed class FakeFileMultipartUploadCompleter : IFileMultipartUploadCompleter
{
    private static readonly ConcurrentBag<CompletedUpload> CompletedUploads = [];

    public static IReadOnlyCollection<CompletedUpload> Uploads => CompletedUploads.ToArray();

    public static void Reset()
    {
        while (CompletedUploads.TryTake(out _))
        {
        }
    }

    public Task CompleteMultipartUploadAsync(
        string storageKey,
        string uploadId,
        IReadOnlyList<CompletedMultipartUploadPart> parts,
        CancellationToken cancellationToken
    )
    {
        CompletedUploads.Add(new CompletedUpload(storageKey, uploadId, parts.ToArray()));
        return Task.CompletedTask;
    }
}

public sealed record CompletedUpload(
    string StorageKey,
    string UploadId,
    IReadOnlyList<CompletedMultipartUploadPart> Parts
);

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
