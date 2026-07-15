using MediatR;
using StorageSystem.Application.Interfaces;

namespace StorageSystem.Application.UseCases.Files.CompleteFileUpload;

public sealed record CompleteFileUploadCommand(
    Guid FileId,
    Guid UserId,
    string UploadId,
    IReadOnlyList<CompletedMultipartUploadPart> Parts
) : IRequest<CompleteFileUploadOutput>;
