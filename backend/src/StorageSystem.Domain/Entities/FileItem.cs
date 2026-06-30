using StorageSystem.Domain.SeedWork;
using StorageSystem.Domain.Validation;

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
        Name = name;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        FolderId = folderId;
        UserId = userId;

        Validate();

        Name = name.Trim();
        ContentType = contentType.Trim();
        StorageKey = BuildStorageKey(userId, folderId, Id, Name);
    }

    private void Validate()
    {
        DomainValidation.NotNullOrEmpty(Name, nameof(Name));
        DomainValidation.MaxLength(Name, 255, nameof(Name));
        DomainValidation.NotNullOrEmpty(ContentType, nameof(ContentType));
        DomainValidation.MaxLength(ContentType, 150, nameof(ContentType));
        DomainValidation.GreaterThanZero(SizeBytes, nameof(SizeBytes));
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
