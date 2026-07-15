using FluentValidation;
using MediatR;
using StorageSystem.Application.Common.Behaviors;
using StorageSystem.Application.Interfaces;
using StorageSystem.Application.UseCases.Folders.CreateFolder;
using StorageSystem.Domain.Repositories;
using StorageSystem.Infrastructure.Data.EF.Persistence.UnitOfWork;
using StorageSystem.Infrastructure.Data.EF.Repositories;
using StorageSystem.Infrastructure.Storage.S3;

namespace StorageSystem.Api.Configurations;

public static class UseCasesConfiguration
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddOptions<StorageOptions>()
            .BindConfiguration("Minio")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Endpoint), "Storage endpoint is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.AccessKey), "Storage access key is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.SecretKey), "Storage secret key is required.")
            .Validate(options => options.MultipartPartSizeBytes >= 5L * 1024 * 1024, "Storage multipart part size must be at least 5 MiB.");

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(CreateFolderCommand).Assembly);
        });

        services.AddValidatorsFromAssemblyContaining<CreateFolderCommandValidator>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddRepositories();

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<IFolderRepository, FolderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFileUploadUrlProvider, S3FileStorageProvider>();
        services.AddScoped<IFileDownloadUrlProvider, S3FileStorageProvider>();
        services.AddScoped<IFileMultipartUploadCompleter, S3FileStorageProvider>();
        services.AddScoped<IFileStorageRemover, S3FileStorageProvider>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}
