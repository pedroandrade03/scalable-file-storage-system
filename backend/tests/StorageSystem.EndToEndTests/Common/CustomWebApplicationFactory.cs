using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StorageSystem.Application.Interfaces;
using StorageSystem.Infrastructure.Persistence;

namespace StorageSystem.EndToEndTests.Common;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string DatabaseName = "e2e-tests-db";

    public static readonly InMemoryDatabaseRoot DatabaseRoot = new();

    static CustomWebApplicationFactory()
    {
        // Program reads configuration at service-registration time (before the host is
        // built), so ConfigureAppConfiguration runs too late. Environment variables are
        // picked up by WebApplication.CreateBuilder immediately, making them available.
        SetEnvironmentVariable("ConnectionStrings__DefaultConnection",
            "Host=localhost;Database=e2e;Username=e2e;Password=e2e");
        SetEnvironmentVariable("Keycloak__Authority", "https://localhost/realms/test");
        SetEnvironmentVariable("Keycloak__MetadataAddress",
            "https://localhost/realms/test/.well-known/openid-configuration");
        SetEnvironmentVariable("Keycloak__ValidIssuer", "https://localhost/realms/test");
        SetEnvironmentVariable("Keycloak__ClientId", "test-client");
        SetEnvironmentVariable("Minio__Endpoint", "localhost:9000");
        SetEnvironmentVariable("Minio__AccessKey", "test-access-key");
        SetEnvironmentVariable("Minio__SecretKey", "test-secret-key");
    }

    private static void SetEnvironmentVariable(string key, string value)
        => Environment.SetEnvironmentVariable(key, value);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("EndToEndTest");

        builder.ConfigureTestServices(services =>
        {
            ReplaceDatabase(services);
            ReplaceStorageProviders(services);
            ReplaceAuthentication(services);
        });

        base.ConfigureWebHost(builder);
    }

    private static void ReplaceDatabase(IServiceCollection services)
    {
        var descriptorsToRemove = services.Where(service =>
            service.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
            service.ServiceType == typeof(DbContextOptions) ||
            service.ServiceType == typeof(ApplicationDbContext) ||
            (service.ServiceType.IsGenericType &&
             service.ServiceType.Name.StartsWith("IDbContextOptionsConfiguration"))
        ).ToList();

        foreach (var descriptor in descriptorsToRemove)
            services.Remove(descriptor);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(DatabaseName, DatabaseRoot)
        );
    }

    private static void ReplaceStorageProviders(IServiceCollection services)
    {
        services.RemoveAll<IFileUploadUrlProvider>();
        services.RemoveAll<IFileDownloadUrlProvider>();
        services.AddScoped<IFileUploadUrlProvider, FakeFileUploadUrlProvider>();
        services.AddScoped<IFileDownloadUrlProvider, FakeFileDownloadUrlProvider>();
    }

    private static void ReplaceAuthentication(IServiceCollection services)
    {
        services.AddAuthentication(TestAuthHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.SchemeName,
                _ => { }
            );
    }
}
