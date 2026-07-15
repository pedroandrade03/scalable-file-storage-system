using FluentAssertions;
using StorageSystem.Domain.Exceptions;
using DomainEntity = StorageSystem.Domain.Entities;

namespace StorageSystem.UnitTests.Domain.Entities.Folder;

[Collection(nameof(FolderTestFixture))]
public class FolderTest
{
    private readonly FolderTestFixture _folderTestFixture;

    public FolderTest(FolderTestFixture folderTestFixture) => _folderTestFixture = folderTestFixture;

    [Fact(DisplayName = nameof(Instantiate))]
    [Trait("Domain", "Folder - Aggregate")]
    public void Instantiate()
    {
        var validFolder = _folderTestFixture.GetValidFolder();
        var dateTimeBefore = DateTime.Now;

        var folder = new DomainEntity.Folder(
            validFolder.Name,
            validFolder.UserId,
            validFolder.ParentFolderId
        );
        var dateTimeAfter = DateTime.Now.AddSeconds(1);

        folder.Should().NotBeNull();
        folder.Name.Should().Be(validFolder.Name);
        folder.UserId.Should().Be(validFolder.UserId);
        folder.ParentFolderId.Should().Be(validFolder.ParentFolderId);
        folder.Id.Should().NotBe(Guid.Empty);
        folder.CreatedAt.Should().NotBeSameDateAs(default);
        (folder.CreatedAt >= dateTimeBefore).Should().BeTrue();
        (folder.CreatedAt <= dateTimeAfter).Should().BeTrue();
    }

    [Theory(DisplayName = nameof(InstantiateErrorWhenNameIsEmpty))]
    [Trait("Domain", "Folder - Aggregate")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void InstantiateErrorWhenNameIsEmpty(string? name)
    {
        var validFolder = _folderTestFixture.GetValidFolder();

        var action = () => new DomainEntity.Folder(
            name!,
            validFolder.UserId,
            validFolder.ParentFolderId
        );

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage("Name should not be empty or null.");
    }

    [Fact(DisplayName = nameof(RenameErrorWhenNameIsEmpty))]
    [Trait("Domain", "Folder - Aggregate")]
    public void RenameErrorWhenNameIsEmpty()
    {
        var folder = _folderTestFixture.GetValidFolder();

        var action = () => folder.Rename(" ");

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage("Name should not be empty or null.");
    }
}
