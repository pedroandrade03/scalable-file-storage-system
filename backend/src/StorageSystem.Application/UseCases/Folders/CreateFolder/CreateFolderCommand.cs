using MediatR;

namespace StorageSystem.Application.UseCases.Folders.CreateFolder;

public sealed record CreateFolderCommand(
    string Name,
    Guid UserId,
    Guid? ParentFolderId
) : IRequest<CreateFolderOutput>;
