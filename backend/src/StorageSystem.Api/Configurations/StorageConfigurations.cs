using Amazon.Runtime;
using Amazon.S3;

namespace StorageSystem.Api.Configurations;

public static class StorageConfigurations
{
    public static IServiceCollection AddAppStorage(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddS3Client(configuration);
        return services;
    }

    private static IServiceCollection AddS3Client(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var accessKey = configuration["MinIO:AccessKey"];
        var secretKey = configuration["MinIO:SecretKey"];
        var endpoint = configuration["MinIO:Endpoint"];
        var region = configuration["MinIO:Region"] ?? "us-east-1";
        var useSsl = configuration.GetValue("MinIO:UseSsl", false);

        if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException(
                "As credenciais 'Storage:AccessKey' ou 'Storage:SecretKey' não foram configuradas."
            );
        }

        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        
        var config = new AmazonS3Config
        {
            AuthenticationRegion = region
        };

        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            var normalizedEndpoint = NormalizeEndpoint(endpoint, useSsl);
            config.ServiceURL = normalizedEndpoint;
            
            config.ForcePathStyle = true; 
            
            config.UseHttp = normalizedEndpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
        }

        services.AddSingleton<IAmazonS3>(new AmazonS3Client(credentials, config));

        return services;
    }

    private static string NormalizeEndpoint(string endpoint, bool useSsl)
    {
        var trimmedEndpoint = endpoint.Trim();
        if (trimmedEndpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || trimmedEndpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return trimmedEndpoint;
        }

        var scheme = useSsl ? "https" : "http";
        return $"{scheme}://{trimmedEndpoint}";
    }
}
