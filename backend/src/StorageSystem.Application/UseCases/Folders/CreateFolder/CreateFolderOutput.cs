using StorageSystem.Domain.Entities;

namespace StorageSystem.Application.UseCases.Folders.CreateFolder;

public sealed record CreateFolderOutput(
    Guid Id,
    string Name,
    Guid UserId,
    Guid? ParentFolderId
)
{
    public static CreateFolderOutput FromFolder(Folder folder)
        => new(
            folder.Id,
            folder.Name,
            folder.UserId,
            folder.ParentFolderId
        );
}
