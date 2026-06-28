using StorageSystem.Domain.Entities;

namespace StorageSystem.Application.UseCases.Files.CreateFile;

public sealed record CreateFileOutput(
    Guid Id,
    string Name,
    string StorageKey,
    string ContentType,
    long SizeBytes,
    Guid FolderId,
    Guid UserId,
    string UploadUrl
)
{
    public static CreateFileOutput FromFile(FileItem file, string uploadUrl)
        => new(
            file.Id,
            file.Name,
            file.StorageKey,
            file.ContentType,
            file.SizeBytes,
            file.FolderId,
            file.UserId,
            uploadUrl
        );
}
