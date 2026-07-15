using MediatR;
using StorageSystem.Application.Exceptions;
using StorageSystem.Application.Interfaces;
using StorageSystem.Domain.Repositories;

namespace StorageSystem.Application.UseCases.Files.CompleteFileUpload;

public class CompleteFileUploadCommandHandler(
    IFileRepository fileRepository,
    IFileMultipartUploadCompleter uploadCompleter,
    IUnitOfWork unitOfWork
) : IRequestHandler<CompleteFileUploadCommand, CompleteFileUploadOutput>
{
    public async Task<CompleteFileUploadOutput> Handle(
        CompleteFileUploadCommand request,
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

        await uploadCompleter.CompleteMultipartUploadAsync(
            file.StorageKey,
            request.UploadId,
            request.Parts,
            cancellationToken
        );

        file.MarkUploadCompleted();
        await unitOfWork.CommitAsync(cancellationToken);

        return CompleteFileUploadOutput.FromFile(file);
    }
}
