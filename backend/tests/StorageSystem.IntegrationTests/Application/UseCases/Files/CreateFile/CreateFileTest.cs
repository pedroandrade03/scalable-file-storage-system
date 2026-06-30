using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using StorageSystem.Application.Exceptions;
using StorageSystem.Application.Interfaces;
using StorageSystem.Application.UseCases.Files.CreateFile;
using StorageSystem.Domain.Exceptions;

namespace StorageSystem.IntegrationTests.Application.UseCases.Files.CreateFile;

[Collection(nameof(CreateFileTestFixture))]
public class CreateFileTest
{
    private readonly CreateFileTestFixture _fixture;

    public CreateFileTest(CreateFileTestFixture fixture) => _fixture = fixture;

    private static Mock<IFileUploadUrlProvider> GetUploadUrlProviderMock(string uploadUrl)
    {
        var mock = new Mock<IFileUploadUrlProvider>();
        mock.Setup(p => p.CreateUploadUrlAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(uploadUrl);
        return mock;
    }

    [Fact(DisplayName = nameof(CreateFile))]
    [Trait("Integration/Application", "CreateFile - Use Cases")]
    public async Task CreateFile()
    {
        var dbContext = _fixture.CreateDbContext();
        var user = _fixture.GetExampleUser();
        var folder = _fixture.GetExampleFolder(user.Id);
        await dbContext.Users.AddAsync(user);
        await dbContext.Folders.AddAsync(folder);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        const string uploadUrl = "https://minio.local/upload";
        var handler = new CreateFileCommandHandler(
            _fixture.CreateFileRepository(dbContext),
            _fixture.CreateFolderRepository(dbContext),
            _fixture.CreateUserRepository(dbContext),
            GetUploadUrlProviderMock(uploadUrl).Object,
            _fixture.CreateUnitOfWork(dbContext)
        );
        var command = _fixture.GetValidCommand(user.Id, folder.Id);

        var output = await handler.Handle(command, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().NotBe(Guid.Empty);
        output.Name.Should().Be(command.Name);
        output.UploadUrl.Should().Be(uploadUrl);
        output.UserId.Should().Be(user.Id);
        output.FolderId.Should().Be(folder.Id);

        var dbFile = await _fixture.CreateDbContext(true).Files.FindAsync(output.Id);
        dbFile.Should().NotBeNull();
        dbFile!.Name.Should().Be(command.Name);
        dbFile.StorageKey.Should().Be(output.StorageKey);
    }

    [Fact(DisplayName = nameof(ThrowWhenFolderNotFound))]
    [Trait("Integration/Application", "CreateFile - Use Cases")]
    public async Task ThrowWhenFolderNotFound()
    {
        var dbContext = _fixture.CreateDbContext();
        var user = _fixture.GetExampleUser();
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateFileCommandHandler(
            _fixture.CreateFileRepository(dbContext),
            _fixture.CreateFolderRepository(dbContext),
            _fixture.CreateUserRepository(dbContext),
            GetUploadUrlProviderMock("https://minio.local/upload").Object,
            _fixture.CreateUnitOfWork(dbContext)
        );
        var command = _fixture.GetValidCommand(user.Id, Guid.NewGuid());

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Folder '{command.FolderId}' was not found.");

        var dbFiles = _fixture.CreateDbContext(true).Files.AsNoTracking().ToList();
        dbFiles.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(ThrowWhenFileNameAlreadyExists))]
    [Trait("Integration/Application", "CreateFile - Use Cases")]
    public async Task ThrowWhenFileNameAlreadyExists()
    {
        var dbContext = _fixture.CreateDbContext();
        var user = _fixture.GetExampleUser();
        var folder = _fixture.GetExampleFolder(user.Id);
        var command = _fixture.GetValidCommand(user.Id, folder.Id);
        var existingFile = new StorageSystem.Domain.Entities.FileItem(
            command.Name,
            command.ContentType,
            command.SizeBytes,
            folder.Id,
            user.Id
        );
        await dbContext.Users.AddAsync(user);
        await dbContext.Folders.AddAsync(folder);
        await dbContext.Files.AddAsync(existingFile);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateFileCommandHandler(
            _fixture.CreateFileRepository(dbContext),
            _fixture.CreateFolderRepository(dbContext),
            _fixture.CreateUserRepository(dbContext),
            GetUploadUrlProviderMock("https://minio.local/upload").Object,
            _fixture.CreateUnitOfWork(dbContext)
        );

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage($"File '{command.Name}' already exists in this folder.");
    }

    [Theory(DisplayName = nameof(ThrowWhenCantInstantiateFile))]
    [Trait("Integration/Application", "CreateFile - Use Cases")]
    [MemberData(
        nameof(CreateFileTestDataGenerator.GetInvalidInputs),
        MemberType = typeof(CreateFileTestDataGenerator)
    )]
    public async Task ThrowWhenCantInstantiateFile(
        string name,
        string contentType,
        long sizeBytes,
        string expectedMessage
    )
    {
        var dbContext = _fixture.CreateDbContext();
        var user = _fixture.GetExampleUser();
        var folder = _fixture.GetExampleFolder(user.Id);
        await dbContext.Users.AddAsync(user);
        await dbContext.Folders.AddAsync(folder);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateFileCommandHandler(
            _fixture.CreateFileRepository(dbContext),
            _fixture.CreateFolderRepository(dbContext),
            _fixture.CreateUserRepository(dbContext),
            GetUploadUrlProviderMock("https://minio.local/upload").Object,
            _fixture.CreateUnitOfWork(dbContext)
        );
        var command = new CreateFileCommand(name, contentType, sizeBytes, folder.Id, user.Id);

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<EntityValidationException>()
            .WithMessage(expectedMessage);

        var dbFiles = _fixture.CreateDbContext(true).Files.AsNoTracking().ToList();
        dbFiles.Should().BeEmpty();
    }
}
