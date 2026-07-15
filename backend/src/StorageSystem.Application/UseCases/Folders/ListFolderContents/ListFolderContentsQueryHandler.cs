using MediatR;
using StorageSystem.Application.Exceptions;
using StorageSystem.Domain.Repositories;

namespace StorageSystem.Application.UseCases.Folders.ListFolderContents;

public class ListFolderContentsQueryHandler(
    IFolderRepository folderRepository,
    IFileRepository fileRepository
) : IRequestHandler<ListFolderContentsQuery, ListFolderContentsOutput>
{
    public async Task<ListFolderContentsOutput> Handle(
        ListFolderContentsQuery request,
        CancellationToken cancellationToken
    )
    {
        if (request.ParentFolderId is not null)
        {
            var parentFolder = await folderRepository.GetByIdAndUserIdAsync(
                request.ParentFolderId.Value,
                request.UserId,
                cancellationToken
            );

            if (parentFolder is null)
            {
                throw new NotFoundException($"Folder '{request.ParentFolderId}' was not found.");
            }
        }

        var folders = await folderRepository.ListByParentAsync(
            request.UserId,
            request.ParentFolderId,
            cancellationToken
        );

        var files = request.ParentFolderId is null
            ? []
            : await fileRepository.ListByFolderAsync(
                request.UserId,
                request.ParentFolderId.Value,
                cancellationToken
            );

        return new ListFolderContentsOutput(
            request.ParentFolderId,
            folders.Select(ListFolderContentsFolderOutput.FromFolder).ToList(),
            files.Select(ListFolderContentsFileOutput.FromFile).ToList()
        );
    }
}
