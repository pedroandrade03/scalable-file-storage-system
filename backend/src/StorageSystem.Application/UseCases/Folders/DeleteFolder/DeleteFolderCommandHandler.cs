using MediatR;
using StorageSystem.Application.Exceptions;
using StorageSystem.Application.Interfaces;
using StorageSystem.Domain.Repositories;

namespace StorageSystem.Application.UseCases.Folders.DeleteFolder;

public class DeleteFolderCommandHandler(
    IFolderRepository folderRepository,
    IFileRepository fileRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteFolderCommand>
{
    public async Task Handle(
        DeleteFolderCommand request,
        CancellationToken cancellationToken
    )
    {
        var folder = await folderRepository.GetByIdAndUserIdAsync(
            request.FolderId,
            request.UserId,
            cancellationToken
        );

        if (folder is null)
        {
            throw new NotFoundException($"Folder '{request.FolderId}' was not found.");
        }

        var hasFiles = await fileRepository.ExistsInFolderAsync(
            request.FolderId,
            request.UserId,
            cancellationToken
        );
        if (hasFiles)
        {
            throw new ConflictException(
                $"Folder '{request.FolderId}' cannot be deleted because it contains files."
            );
        }

        var hasSubFolders = await folderRepository.HasSubFoldersAsync(
            request.FolderId,
            request.UserId,
            cancellationToken
        );
        if (hasSubFolders)
        {
            throw new ConflictException(
                $"Folder '{request.FolderId}' cannot be deleted because it contains subfolders."
            );
        }

        await folderRepository.DeleteAsync(folder, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
    }
}
