namespace StorageSystem.Application.Interfaces;

public interface IFileStorageRemover
{
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken);
}
