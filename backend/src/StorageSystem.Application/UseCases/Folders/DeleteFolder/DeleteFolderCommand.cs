using MediatR;

namespace StorageSystem.Application.UseCases.Folders.DeleteFolder;

public sealed record DeleteFolderCommand(Guid FolderId, Guid UserId) : IRequest;
