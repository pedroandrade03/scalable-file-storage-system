using StorageSystem.Api.Filters;
using StorageSystem.Api.Services;
using StorageSystem.Application.Interfaces;

namespace StorageSystem.Api.Configurations;

public static class ControllersConfiguration
{
    public static IServiceCollection AddAndConfigureControllers(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();

        services.AddControllers(options =>
        {
            options.Filters.Add<ApiGlobalExceptionFilter>();
        });

        services.AddProblemDetails();
        services.ConfigureCors(configuration);

        return services;
    }

    private static IServiceCollection ConfigureCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();

        if (allowedOrigins is null || allowedOrigins.Length == 0)
        {
            return services;
        }

        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        return services;
    }
}
