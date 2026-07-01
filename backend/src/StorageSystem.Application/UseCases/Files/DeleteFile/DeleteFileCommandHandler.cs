using MediatR;
using StorageSystem.Application.Exceptions;
using StorageSystem.Application.Interfaces;
using StorageSystem.Domain.Repositories;

namespace StorageSystem.Application.UseCases.Files.DeleteFile;

public class DeleteFileCommandHandler(
    IFileRepository fileRepository,
    IFileStorageRemover fileStorageRemover,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteFileCommand>
{
    public async Task Handle(
        DeleteFileCommand request,
        CancellationToken cancellationToken
    )
    {
        var file = await fileRepository.GetByIdAndUserIdAsync(
            request.FileId,
            request.UserId,
            cancellationToken
        );

        if (file is null)
        {
            throw new NotFoundException($"File '{request.FileId}' was not found.");
        }

        await fileStorageRemover.DeleteAsync(file.StorageKey, cancellationToken);
        await fileRepository.DeleteAsync(file, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
    }
}
