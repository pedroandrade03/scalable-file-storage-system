using StorageSystem.Domain.Entities;
using StorageSystem.Domain.Enums;

namespace StorageSystem.Application.UseCases.Folders.ListFolderContents;

public sealed record ListFolderContentsOutput(
    Guid? ParentFolderId,
    IReadOnlyList<ListFolderContentsFolderOutput> Folders,
    IReadOnlyList<ListFolderContentsFileOutput> Files
);

public sealed record ListFolderContentsFolderOutput(
    Guid Id,
    string Name,
    Guid UserId,
    Guid? ParentFolderId
)
{
    public static ListFolderContentsFolderOutput FromFolder(Folder folder) => new(
        folder.Id,
        folder.Name,
        folder.UserId,
        folder.ParentFolderId
    );
}

public sealed record ListFolderContentsFileOutput(
    Guid Id,
    string Name,
    string StorageKey,
    string ContentType,
    long SizeBytes,
    FileStatus Status,
    Guid FolderId,
    Guid UserId
)
{
    public static ListFolderContentsFileOutput FromFile(FileItem file) => new(
        file.Id,
        file.Name,
        file.StorageKey,
        file.ContentType,
        file.SizeBytes,
        file.Status,
        file.FolderId,
        file.UserId
    );
}
