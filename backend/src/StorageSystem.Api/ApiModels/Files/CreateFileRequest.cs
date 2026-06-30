namespace StorageSystem.Api.ApiModels.Files;

public sealed record CreateFileRequest(
    string Name,
    string ContentType,
    long SizeBytes,
    Guid FolderId
);
