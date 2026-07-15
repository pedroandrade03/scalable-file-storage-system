using StorageSystem.Application.Interfaces;

namespace StorageSystem.Api.ApiModels.Files;

public sealed record CompleteFileUploadRequest(
    string UploadId,
    IReadOnlyList<CompletedMultipartUploadPart> Parts
);
