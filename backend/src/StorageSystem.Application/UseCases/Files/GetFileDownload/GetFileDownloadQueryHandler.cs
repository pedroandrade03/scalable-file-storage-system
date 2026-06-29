using MediatR;
using StorageSystem.Application.Exceptions;
using StorageSystem.Application.Interfaces;
using StorageSystem.Domain.Repositories;

namespace StorageSystem.Application.UseCases.Files.GetFileDownload;

public class GetFileDownloadQueryHandler(
    IFileRepository fileRepository,
    IFileDownloadUrlProvider downloadUrlProvider
) : IRequestHandler<GetFileDownloadQuery, GetFileDownloadOutput>
{
    public async Task<GetFileDownloadOutput> Handle(
        GetFileDownloadQuery request,
        CancellationToken cancellationToken
    )
    {
        var file = await fileRepository.GetByIdAsync(request.FileId, cancellationToken);
        if (file is null)
        {
            throw new NotFoundException($"File '{request.FileId}' was not found.");
        }

        var downloadUrl = await downloadUrlProvider.CreateDownloadUrlAsync(
            file.StorageKey,
            cancellationToken
        );

        return new GetFileDownloadOutput(file.Id, downloadUrl);
    }
}
