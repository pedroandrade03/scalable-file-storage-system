using MediatR;
using StorageSystem.Application.Exceptions;
using StorageSystem.Application.Interfaces;
using StorageSystem.Domain.Repositories;
using DomainEntity = StorageSystem.Domain.Entities;

namespace StorageSystem.Application.UseCases.Files.CreateFile;

public class CreateFileCommandHandler(
    IFileRepository fileRepository,
    IFolderRepository folderRepository,
    IUserRepository userRepository,
    IFileUploadUrlProvider uploadUrlProvider,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateFileCommand, CreateFileOutput>
{
    public async Task<CreateFileOutput> Handle(
        CreateFileCommand request,
        CancellationToken cancellationToken
    )
    {
        var userExists = await userRepository.ExistsAsync(request.UserId, cancellationToken);
        if (!userExists)
        {
            throw new NotFoundException($"User '{request.UserId}' was not found.");
        }

        var folder = await folderRepository.GetByIdAsync(request.FolderId, cancellationToken);
        if (folder is null)
        {
            throw new NotFoundException($"Folder '{request.FolderId}' was not found.");
        }

        if (folder.UserId != request.UserId)
        {
            throw new ApplicationValidationException(
                "Folder must belong to the same user."
            );
        }

        var normalizedName = request.Name.Trim();
        var normalizedContentType = request.ContentType.Trim();
        var nameAlreadyExists = await fileRepository.ExistsByNameAsync(
            request.UserId,
            request.FolderId,
            normalizedName,
            cancellationToken
        );

        if (nameAlreadyExists)
        {
            throw new ConflictException(
                $"File '{normalizedName}' already exists in this folder."
            );
        }

        var file = new DomainEntity.FileItem(
            normalizedName,
            normalizedContentType,
            request.SizeBytes,
            request.FolderId,
            request.UserId
        );

        var uploadUrl = await uploadUrlProvider.CreateUploadUrlAsync(
            file.StorageKey,
            file.ContentType,
            file.SizeBytes,
            cancellationToken
        );

        await fileRepository.InsertAsync(file, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return CreateFileOutput.FromFile(file, uploadUrl);
    }
}
