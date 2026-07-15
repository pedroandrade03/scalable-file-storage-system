using StorageSystem.Domain.Entities;
using StorageSystem.Domain.Enums;

namespace StorageSystem.Application.UseCases.Files.CompleteFileUpload;

public sealed record CompleteFileUploadOutput(
    Guid Id,
    FileStatus Status
)
{
    public static CompleteFileUploadOutput FromFile(FileItem file)
        => new(file.Id, file.Status);
}
