using StorageSystem.Domain.SeedWork;

namespace StorageSystem.Domain.Entities;

public class Folder : AggregateRoot
{
    public string Name { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? ParentFolderId { get; private set; } 

    private readonly List<Folder> _subFolders = new();
    public IReadOnlyCollection<Folder> SubFolders => _subFolders.AsReadOnly();

    public Folder(string name, Guid userId, Guid? parentFolderId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Folder name cannot be empty", nameof(name));

        Name = name.Trim();
        UserId = userId;
        ParentFolderId = parentFolderId;
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Folder name cannot be empty");
            
        Name = newName;
    }
}
