using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StorageSystem.Api.ApiModels.Folders;
using StorageSystem.Api.ApiModels.Response;
using StorageSystem.Application.Interfaces;
using StorageSystem.Application.UseCases.Folders.CreateFolder;

namespace StorageSystem.Api.Controllers;

[ApiController]
[Route("folders")]
[Authorize]
public class FoldersController(
    IMediator mediator,
    ICurrentUserAccessor currentUser
) : ControllerBase
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
        var userId = await currentUser.GetUserIdAsync(cancellationToken);

        var output = await mediator.Send(
            new CreateFolderCommand(
                request.Name,
                userId,
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
