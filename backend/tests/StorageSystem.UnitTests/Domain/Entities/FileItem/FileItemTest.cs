using FluentAssertions;
using StorageSystem.Domain.Exceptions;
using DomainEntity = StorageSystem.Domain.Entities;

namespace StorageSystem.UnitTests.Domain.Entities.FileItem;

[Collection(nameof(FileItemTestFixture))]
public class FileItemTest
{
    private readonly FileItemTestFixture _fileItemTestFixture;

    public FileItemTest(FileItemTestFixture fileItemTestFixture) => _fileItemTestFixture = fileItemTestFixture;

    [Fact(DisplayName = nameof(Instantiate))]
    [Trait("Domain", "FileItem - Aggregate")]
    public void Instantiate()
    {
        var validFile = _fileItemTestFixture.GetValidFileItem();
        var dateTimeBefore = DateTime.Now;

        var file = new DomainEntity.FileItem(
            validFile.Name,
            validFile.ContentType,
            validFile.SizeBytes,
            validFile.FolderId,
            validFile.UserId
        );
        var dateTimeAfter = DateTime.Now.AddSeconds(1);

        file.Should().NotBeNull();
        file.Name.Should().Be(validFile.Name);
        file.ContentType.Should().Be(validFile.ContentType);
        file.SizeBytes.Should().Be(validFile.SizeBytes);
        file.FolderId.Should().Be(validFile.FolderId);
        file.UserId.Should().Be(validFile.UserId);
        file.Id.Should().NotBe(Guid.Empty);
        file.CreatedAt.Should().NotBeSameDateAs(default);
        (file.CreatedAt >= dateTimeBefore).Should().BeTrue();
        (file.CreatedAt <= dateTimeAfter).Should().BeTrue();
        file.StorageKey.Should().Contain(file.UserId.ToString());
        file.StorageKey.Should().Contain(file.FolderId.ToString());
        file.StorageKey.Should().Contain(file.Id.ToString());
        file.StorageKey.Should().EndWith(file.Name);
    }

    [Theory(DisplayName = nameof(InstantiateErrorWhenNameIsEmpty))]
    [Trait("Domain", "FileItem - Aggregate")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void InstantiateErrorWhenNameIsEmpty(string? name)
    {
        var validFile = _fileItemTestFixture.GetValidFileItem();

        var action = () => new DomainEntity.FileItem(
            name!,
            validFile.ContentType,
            validFile.SizeBytes,
            validFile.FolderId,
            validFile.UserId
        );

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage("Name should not be empty or null.");
    }

    [Theory(DisplayName = nameof(InstantiateErrorWhenContentTypeIsEmpty))]
    [Trait("Domain", "FileItem - Aggregate")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void InstantiateErrorWhenContentTypeIsEmpty(string? contentType)
    {
        var validFile = _fileItemTestFixture.GetValidFileItem();

        var action = () => new DomainEntity.FileItem(
            validFile.Name,
            contentType!,
            validFile.SizeBytes,
            validFile.FolderId,
            validFile.UserId
        );

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage("ContentType should not be empty or null.");
    }

    [Theory(DisplayName = nameof(InstantiateErrorWhenSizeBytesIsNotPositive))]
    [Trait("Domain", "FileItem - Aggregate")]
    [InlineData(0)]
    [InlineData(-1)]
    public void InstantiateErrorWhenSizeBytesIsNotPositive(long sizeBytes)
    {
        var validFile = _fileItemTestFixture.GetValidFileItem();

        var action = () => new DomainEntity.FileItem(
            validFile.Name,
            validFile.ContentType,
            sizeBytes,
            validFile.FolderId,
            validFile.UserId
        );

        action.Should()
            .Throw<EntityValidationException>()
            .WithMessage("SizeBytes should be greater than zero.");
    }
}
