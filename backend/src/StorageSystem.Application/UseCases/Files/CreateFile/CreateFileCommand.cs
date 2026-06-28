using MediatR;

namespace StorageSystem.Application.UseCases.Files.CreateFile;

public sealed record CreateFileCommand(
    string Name,
    string ContentType,
    long SizeBytes,
    Guid FolderId,
    Guid UserId
) : IRequest<CreateFileOutput>;
