namespace StorageSystem.Api.ApiModels.Folders;

public sealed record CreateFolderRequest(
    string Name,
    Guid UserId,
    Guid? ParentFolderId
);
