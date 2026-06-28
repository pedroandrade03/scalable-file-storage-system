using StorageSystem.Domain.SeedWork;

namespace StorageSystem.Domain.Entities;

public class User : AggregateRoot
{
    public string Name { get; private set; }
    public string Email { get; private set; }

    public User(string name, string email)
    {
        Name = name;
        Email = email;
    }
}