using StorageSystem.Domain.Entities;
using StorageSystem.Application.Interfaces;
using StorageSystem.Domain.Enums;

namespace StorageSystem.Application.UseCases.Files.CreateFile;

public sealed record CreateFileOutput(
    Guid Id,
    string Name,
    string StorageKey,
    string ContentType,
    long SizeBytes,
    FileStatus Status,
    Guid FolderId,
    Guid UserId,
    MultipartUploadPlan Upload
)
{
    public static CreateFileOutput FromFile(FileItem file, MultipartUploadPlan upload)
        => new(
            file.Id,
            file.Name,
            file.StorageKey,
            file.ContentType,
            file.SizeBytes,
            file.Status,
            file.FolderId,
            file.UserId,
            upload
        );
}
