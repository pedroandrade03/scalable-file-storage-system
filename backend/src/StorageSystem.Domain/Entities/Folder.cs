using StorageSystem.Domain.SeedWork;
using StorageSystem.Domain.Validation;

namespace StorageSystem.Domain.Entities;

public sealed class Folder : AggregateRoot
{
    public string Name { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public Guid? ParentFolderId { get; private set; }

    private Folder()
    {
    }

    public Folder(string name, Guid userId, Guid? parentFolderId = null)
    {
        Name = name;
        UserId = userId;
        ParentFolderId = parentFolderId;

        Validate();

        Name = name.Trim();
    }

    public void Rename(string newName)
    {
        Name = newName;

        Validate();

        Name = newName.Trim();
    }

    private void Validate()
    {
        DomainValidation.NotNullOrEmpty(Name, nameof(Name));
        DomainValidation.MaxLength(Name, 255, nameof(Name));
    }
}
