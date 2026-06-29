using MediatR;

namespace StorageSystem.Application.UseCases.Files.GetFileDownload;

public sealed record GetFileDownloadQuery(Guid FileId) : IRequest<GetFileDownloadOutput>;
