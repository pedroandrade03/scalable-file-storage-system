using MediatR;

namespace StorageSystem.Application.UseCases.Files.DeleteFile;

public sealed record DeleteFileCommand(
    Guid FileId,
    Guid UserId
) : IRequest;
