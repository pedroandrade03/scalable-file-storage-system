using System.Security.Claims;
using StorageSystem.Application.Interfaces;
using StorageSystem.Domain.Entities;
using StorageSystem.Domain.Repositories;

namespace StorageSystem.Api.Services;

public sealed class HttpContextCurrentUserAccessor(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork
) : ICurrentUserAccessor
{
    private const string DefaultExternalProvider = "keycloak";
    private const string LegacyExternalProvider = "legacy";

    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public string? Email =>
        Principal?.FindFirstValue(ClaimTypes.Email)
        ?? Principal?.FindFirstValue("email");

    public string? Name =>
        Principal?.FindFirstValue(ClaimTypes.Name)
        ?? Principal?.FindFirstValue("name")
        ?? Principal?.FindFirstValue("preferred_username");

    public string? Subject =>
        Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? Principal?.FindFirstValue("sub")
        ?? Principal?.FindFirstValue("preferred_username");

    public async Task<Guid> GetUserIdAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated)
        {
            throw new UnauthorizedAccessException();
        }

        var email = Email;
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new UnauthorizedAccessException("Email claim is missing from the access token.");
        }

        var subject = Subject;
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new UnauthorizedAccessException("Subject claim is missing from the access token.");
        }

        var externalProvider = GetExternalProvider();
        var name = Name ?? email;
        var user = await userRepository.GetByExternalIdentityAsync(
            externalProvider,
            subject,
            cancellationToken
        );

        if (user is null)
        {
            user = await userRepository.GetByEmailAsync(email, cancellationToken);

            if (user is not null && user.ExternalProvider != LegacyExternalProvider)
            {
                throw new UnauthorizedAccessException(
                    "Email claim is already linked to another external identity."
                );
            }
        }

        if (user is null)
        {
            user = new User(name, email, externalProvider, subject);
            await userRepository.InsertAsync(user, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            return user.Id;
        }

        var shouldCommit = false;
        if (user.ExternalProvider == LegacyExternalProvider)
        {
            user.AssignExternalIdentity(externalProvider, subject);
            shouldCommit = true;
        }

        if (user.Name != name || user.Email != email)
        {
            await EnsureEmailCanBeUsedAsync(user, email, cancellationToken);
            user.UpdateProfile(name, email);
            shouldCommit = true;
        }

        if (shouldCommit)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return user.Id;
    }

    private string GetExternalProvider()
    {
        var configuredProvider = configuration["Authentication:ExternalProvider"];
        return string.IsNullOrWhiteSpace(configuredProvider)
            ? DefaultExternalProvider
            : configuredProvider.Trim();
    }

    private async Task EnsureEmailCanBeUsedAsync(
        User user,
        string email,
        CancellationToken cancellationToken
    )
    {
        if (user.Email == email)
        {
            return;
        }

        var userWithSameEmail = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (userWithSameEmail is not null && userWithSameEmail.Id != user.Id)
        {
            throw new UnauthorizedAccessException(
                "Email claim is already linked to another user."
            );
        }
    }
}
