namespace StorageSystem.Application.Interfaces;

public interface IFileMultipartUploadCompleter
{
    Task CompleteMultipartUploadAsync(
        string storageKey,
        string uploadId,
        IReadOnlyList<CompletedMultipartUploadPart> parts,
        CancellationToken cancellationToken
    );
}

public sealed record CompletedMultipartUploadPart(
    int PartNumber,
    string ETag
);
