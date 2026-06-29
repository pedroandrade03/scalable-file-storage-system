namespace StorageSystem.Application.UseCases.Files.GetFileDownload;

public sealed record GetFileDownloadOutput(
    Guid FileId,
    string DownloadUrl
);
