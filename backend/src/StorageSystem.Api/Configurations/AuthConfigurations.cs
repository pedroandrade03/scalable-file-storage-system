using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace StorageSystem.Api.Configurations
{
    public static class AuthConfigurations
    {
        public static IServiceCollection AddAndConfigureAuth(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddAuthorization();
            services.ConfigureAuthentication(configuration);

            return services;
        }

        private static IServiceCollection ConfigureAuthentication(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var authority = configuration["Keycloak:Authority"]
                ?? throw new InvalidOperationException("Keycloak:Authority is required.");
            var metadataAddress = configuration["Keycloak:MetadataAddress"]
                ?? throw new InvalidOperationException("Keycloak:MetadataAddress is required.");
            var validIssuer = configuration["Keycloak:ValidIssuer"] ?? authority;
            var validClientId = configuration["Keycloak:ClientId"]
                ?? throw new InvalidOperationException("Keycloak:ClientId is required.");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = authority;
                    options.MetadataAddress = metadataAddress;
                    options.RequireHttpsMetadata = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = validIssuer,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        NameClaimType = "preferred_username"
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var authorizedParty = context.Principal?.FindFirst("azp")?.Value;
                            if (authorizedParty != validClientId)
                            {
                                context.Fail(
                                    $"The authorized party '{authorizedParty ?? "empty"}' is invalid."
                                );
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }
    }
}
