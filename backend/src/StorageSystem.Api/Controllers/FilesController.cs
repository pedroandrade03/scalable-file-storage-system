using MediatR;
using Microsoft.AspNetCore.Mvc;
using StorageSystem.Api.ApiModels.Files;
using StorageSystem.Api.ApiModels.Response;
using StorageSystem.Application.UseCases.Files.CreateFile;

namespace StorageSystem.Api.Controllers;

[ApiController]
[Route("files")]
public class FilesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateFileOutput>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateFileRequest request,
        CancellationToken cancellationToken
    )
    {
        var output = await mediator.Send(
            new CreateFileCommand(
                request.Name,
                request.ContentType,
                request.SizeBytes,
                request.FolderId,
                request.UserId
            ),
            cancellationToken
        );

        return Created(
            $"/files/{output.Id}",
            new ApiResponse<CreateFileOutput>(output)
        );
    }
}
