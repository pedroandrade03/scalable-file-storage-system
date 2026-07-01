using FluentValidation;
using MediatR;
using StorageSystem.Application.Common.Behaviors;
using StorageSystem.Application.Interfaces;
using StorageSystem.Application.UseCases.Folders.CreateFolder;
using StorageSystem.Domain.Repositories;
using StorageSystem.Infrastructure.Data.EF.Persistence.UnitOfWork;
using StorageSystem.Infrastructure.Data.EF.Repositories;
using StorageSystem.Infrastructure.Storage.Minio;

namespace StorageSystem.Api.Configurations;

public static class UseCasesConfiguration
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddOptions<MinioOptions>()
            .BindConfiguration("Minio")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Endpoint), "Minio endpoint is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.AccessKey), "Minio access key is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.SecretKey), "Minio secret key is required.");

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
        services.AddScoped<IFileUploadUrlProvider, MinioFileUploadUrlProvider>();
        services.AddScoped<IFileDownloadUrlProvider, MinioFileUploadUrlProvider>();
        services.AddScoped<IFileStorageRemover, MinioFileUploadUrlProvider>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}
