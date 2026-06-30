using StorageSystem.Domain.SeedWork;
using StorageSystem.Domain.Validation;

namespace StorageSystem.Domain.Entities;

public class User : AggregateRoot
{
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string ExternalProvider { get; private set; } = null!;
    public string ExternalSubject { get; private set; } = null!;

    private User()
    {
    }

    public User(string name, string email, string externalProvider, string externalSubject)
    {
        Name = name;
        Email = email;
        ExternalProvider = externalProvider;
        ExternalSubject = externalSubject;

        Validate();

        TrimProperties();
    }

    public void UpdateProfile(string name, string email)
    {
        Name = name;
        Email = email;

        ValidateProfile();

        Name = Name.Trim();
        Email = Email.Trim();
        UpdatedAt = DateTimeOffset.Now;
    }

    public void AssignExternalIdentity(string externalProvider, string externalSubject)
    {
        ExternalProvider = externalProvider;
        ExternalSubject = externalSubject;

        ValidateExternalIdentity();

        ExternalProvider = ExternalProvider.Trim();
        ExternalSubject = ExternalSubject.Trim();
        UpdatedAt = DateTimeOffset.Now;
    }

    private void Validate()
    {
        ValidateProfile();
        ValidateExternalIdentity();
    }

    private void ValidateProfile()
    {
        DomainValidation.NotNullOrEmpty(Name, nameof(Name));
        DomainValidation.MaxLength(Name, 150, nameof(Name));
        DomainValidation.NotNullOrEmpty(Email, nameof(Email));
        DomainValidation.MaxLength(Email, 320, nameof(Email));
    }

    private void ValidateExternalIdentity()
    {
        DomainValidation.NotNullOrEmpty(ExternalProvider, nameof(ExternalProvider));
        DomainValidation.MaxLength(ExternalProvider, 100, nameof(ExternalProvider));
        DomainValidation.NotNullOrEmpty(ExternalSubject, nameof(ExternalSubject));
        DomainValidation.MaxLength(ExternalSubject, 200, nameof(ExternalSubject));
    }

    private void TrimProperties()
    {
        Name = Name.Trim();
        Email = Email.Trim();
        ExternalProvider = ExternalProvider.Trim();
        ExternalSubject = ExternalSubject.Trim();
    }
}
