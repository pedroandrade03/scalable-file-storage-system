using Microsoft.OpenApi;

namespace StorageSystem.Api.Configurations;

public static class DocumentationConfiguration
{
    public static IServiceCollection AddAndConfigureDocumentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var keycloakAuthority = configuration["Keycloak:Authority"];

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "StorageSystem API", Version = "v1" });

            options.AddSecurityDefinition(nameof(SecuritySchemeType.OAuth2), new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{keycloakAuthority}/protocol/openid-connect/auth"),
                        TokenUrl = new Uri($"{keycloakAuthority}/protocol/openid-connect/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "OpenID Connect scope" },
                            { "profile", "User profile" }
                        }
                    }
                }
            });

            options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference(nameof(SecuritySchemeType.OAuth2), doc),
                    []
                }
            });
        });

        return services;
    }

    public static WebApplication UseDocumentation(
        this WebApplication app,
        IConfiguration configuration)
    {
        var keycloakClientId = configuration["Keycloak:ClientId"];

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "StorageSystem API v1");
                options.OAuthClientId(keycloakClientId);
                options.OAuthUsePkce();
            });
        }

        return app;
    }
}
