using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StorageSystem.Application.Exceptions;
using StorageSystem.Application.UseCases.Folders.CreateFolder;
using StorageSystem.Domain.Exceptions;

namespace StorageSystem.IntegrationTests.Application.UseCases.Folders.CreateFolder;

[Collection(nameof(CreateFolderTestFixture))]
public class CreateFolderTest
{
    private readonly CreateFolderTestFixture _fixture;

    public CreateFolderTest(CreateFolderTestFixture fixture) => _fixture = fixture;

    [Fact(DisplayName = nameof(CreateFolder))]
    [Trait("Integration/Application", "CreateFolder - Use Cases")]
    public async Task CreateFolder()
    {
        var dbContext = _fixture.CreateDbContext();
        var user = _fixture.GetExampleUser();
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateFolderCommandHandler(
            _fixture.CreateFolderRepository(dbContext),
            _fixture.CreateUserRepository(dbContext),
            _fixture.CreateUnitOfWork(dbContext)
        );
        var command = _fixture.GetValidCommand(user.Id);

        var output = await handler.Handle(command, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().NotBe(Guid.Empty);
        output.Name.Should().Be(command.Name);
        output.UserId.Should().Be(user.Id);
        output.ParentFolderId.Should().BeNull();

        var dbFolder = await _fixture.CreateDbContext(true).Folders.FindAsync(output.Id);
        dbFolder.Should().NotBeNull();
        dbFolder!.Name.Should().Be(command.Name);
        dbFolder.UserId.Should().Be(user.Id);
    }

    [Fact(DisplayName = nameof(CreateSubFolder))]
    [Trait("Integration/Application", "CreateFolder - Use Cases")]
    public async Task CreateSubFolder()
    {
        var dbContext = _fixture.CreateDbContext();
        var user = _fixture.GetExampleUser();
        var parentFolder = _fixture.GetExampleFolder(user.Id);
        await dbContext.Users.AddAsync(user);
        await dbContext.Folders.AddAsync(parentFolder);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateFolderCommandHandler(
            _fixture.CreateFolderRepository(dbContext),
            _fixture.CreateUserRepository(dbContext),
            _fixture.CreateUnitOfWork(dbContext)
        );
        var command = _fixture.GetValidCommand(user.Id, parentFolder.Id);

        var output = await handler.Handle(command, CancellationToken.None);

        output.ParentFolderId.Should().Be(parentFolder.Id);

        var dbFolder = await _fixture.CreateDbContext(true).Folders.FindAsync(output.Id);
        dbFolder.Should().NotBeNull();
        dbFolder!.ParentFolderId.Should().Be(parentFolder.Id);
    }

    [Fact(DisplayName = nameof(ThrowWhenUserNotFound))]
    [Trait("Integration/Application", "CreateFolder - Use Cases")]
    public async Task ThrowWhenUserNotFound()
    {
        var dbContext = _fixture.CreateDbContext();
        var handler = new CreateFolderCommandHandler(
            _fixture.CreateFolderRepository(dbContext),
            _fixture.CreateUserRepository(dbContext),
            _fixture.CreateUnitOfWork(dbContext)
        );
        var command = _fixture.GetValidCommand(Guid.NewGuid());

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"User '{command.UserId}' was not found.");

        var dbFolders = _fixture.CreateDbContext(true).Folders.AsNoTracking().ToList();
        dbFolders.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(ThrowWhenFolderNameAlreadyExists))]
    [Trait("Integration/Application", "CreateFolder - Use Cases")]
    public async Task ThrowWhenFolderNameAlreadyExists()
    {
        var dbContext = _fixture.CreateDbContext();
        var user = _fixture.GetExampleUser();
        var command = _fixture.GetValidCommand(user.Id);
        var existingFolder = new StorageSystem.Domain.Entities.Folder(command.Name, user.Id);
        await dbContext.Users.AddAsync(user);
        await dbContext.Folders.AddAsync(existingFolder);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateFolderCommandHandler(
            _fixture.CreateFolderRepository(dbContext),
            _fixture.CreateUserRepository(dbContext),
            _fixture.CreateUnitOfWork(dbContext)
        );

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage($"Folder '{command.Name}' already exists in this location.");
    }

    [Theory(DisplayName = nameof(ThrowWhenCantInstantiateFolder))]
    [Trait("Integration/Application", "CreateFolder - Use Cases")]
    [MemberData(
        nameof(CreateFolderTestDataGenerator.GetInvalidNames),
        MemberType = typeof(CreateFolderTestDataGenerator)
    )]
    public async Task ThrowWhenCantInstantiateFolder(string invalidName, string expectedMessage)
    {
        var dbContext = _fixture.CreateDbContext();
        var user = _fixture.GetExampleUser();
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateFolderCommandHandler(
            _fixture.CreateFolderRepository(dbContext),
            _fixture.CreateUserRepository(dbContext),
            _fixture.CreateUnitOfWork(dbContext)
        );
        var command = new CreateFolderCommand(invalidName, user.Id, null);

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<EntityValidationException>()
            .WithMessage(expectedMessage);

        var dbFolders = _fixture.CreateDbContext(true).Folders.AsNoTracking().ToList();
        dbFolders.Should().BeEmpty();
    }
}
