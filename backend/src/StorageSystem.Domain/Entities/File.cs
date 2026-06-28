using StorageSystem.Domain.SeedWork;

namespace StorageSystem.Domain.Entities;

public class FileItem : AggregateRoot
{
    public string Name { get; private set; } = null!;
    public string StorageKey { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long SizeBytes { get; private set; }

    public Guid FolderId { get; private set; }
    public Guid UserId { get; private set; }

    private FileItem()
    {
    }

    public FileItem(
        string name,
        string contentType,
        long sizeBytes,
        Guid folderId,
        Guid userId
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("File name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type cannot be empty", nameof(contentType));

        if (sizeBytes <= 0)
            throw new ArgumentException("File size must be greater than zero", nameof(sizeBytes));

        Name = name.Trim();
        ContentType = contentType.Trim();
        SizeBytes = sizeBytes;
        FolderId = folderId;
        UserId = userId;
        StorageKey = BuildStorageKey(userId, folderId, Id, Name);
    }

    private static string BuildStorageKey(
        Guid userId,
        Guid folderId,
        Guid fileId,
        string fileName
    )
    {
        var safeFileName = Path.GetFileName(fileName);
        return $"users/{userId}/folders/{folderId}/files/{fileId}/{safeFileName}";
    }
}
