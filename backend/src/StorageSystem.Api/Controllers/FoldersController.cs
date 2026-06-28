using MediatR;
using Microsoft.AspNetCore.Mvc;
using StorageSystem.Api.ApiModels.Folders;
using StorageSystem.Api.ApiModels.Response;
using StorageSystem.Application.UseCases.Folders.CreateFolder;

namespace StorageSystem.Api.Controllers;

[ApiController]
[Route("folders")]
public class FoldersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateFolderOutput>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateFolderRequest request,
        CancellationToken cancellationToken
    )
    {
        var output = await mediator.Send(
            new CreateFolderCommand(
                request.Name,
                request.UserId,
                request.ParentFolderId
            ),
            cancellationToken
        );

        return Created(
            $"/folders/{output.Id}",
            new ApiResponse<CreateFolderOutput>(output)
        );
    }
}
