namespace StorageSystem.Application.Interfaces;

public interface ICurrentUserAccessor
{
    string? Email { get; }
    string? Name { get; }
    string? Subject { get; }
    bool IsAuthenticated { get; }
    Task<Guid> GetUserIdAsync(CancellationToken cancellationToken = default);
}
