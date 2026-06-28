using MediatR;
using StorageSystem.Application.Exceptions;
using StorageSystem.Application.Interfaces;
using StorageSystem.Domain.Repositories;
using DomainEntity = StorageSystem.Domain.Entities;

namespace StorageSystem.Application.UseCases.Folders.CreateFolder;

public class CreateFolderCommandHandler(
    IFolderRepository folderRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateFolderCommand, CreateFolderOutput>
{
    public async Task<CreateFolderOutput> Handle(
        CreateFolderCommand request,
        CancellationToken cancellationToken
    )
    {
        var userExists = await userRepository.ExistsAsync(request.UserId, cancellationToken);
        if (!userExists)
        {
            throw new NotFoundException($"User '{request.UserId}' was not found.");
        }

        if (request.ParentFolderId is not null)
        {
            var parentFolder = await folderRepository.GetByIdAsync(
                request.ParentFolderId.Value,
                cancellationToken
            );

            if (parentFolder is null)
            {
                throw new NotFoundException($"Parent folder '{request.ParentFolderId}' was not found.");
            }

            if (parentFolder.UserId != request.UserId)
            {
                throw new ApplicationValidationException(
                    "Parent folder must belong to the same user."
                );
            }
        }

        var normalizedName = request.Name.Trim();
        var nameAlreadyExists = await folderRepository.ExistsByNameAsync(
            request.UserId,
            request.ParentFolderId,
            normalizedName,
            cancellationToken
        );

        if (nameAlreadyExists)
        {
            throw new ConflictException(
                $"Folder '{normalizedName}' already exists in this location."
            );
        }

        var folder = new DomainEntity.Folder(
            normalizedName,
            request.UserId,
            request.ParentFolderId
        );

        await folderRepository.InsertAsync(folder, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return CreateFolderOutput.FromFolder(folder);
    }
}
