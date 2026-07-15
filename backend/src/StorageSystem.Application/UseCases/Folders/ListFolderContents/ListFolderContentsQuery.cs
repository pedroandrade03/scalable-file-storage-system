using MediatR;

namespace StorageSystem.Application.UseCases.Folders.ListFolderContents;

public sealed record ListFolderContentsQuery(
    Guid UserId,
    Guid? ParentFolderId
) : IRequest<ListFolderContentsOutput>;
