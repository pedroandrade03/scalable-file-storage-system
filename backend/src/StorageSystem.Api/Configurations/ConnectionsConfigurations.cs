using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using StorageSystem.Infrastructure.Data.EF.Persistence.Contexts;

namespace StorageSystem.Api.Configurations
{
    public static class ConnectionsConfigurations
    { 
        public static IServiceCollection AddAppConnections(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddDbConnection(configuration);
            return services;
        }

        public static WebApplication ApplyDatabaseMigrations(this WebApplication app)
        {
            if (!app.Configuration.GetValue("ApplyMigrations", false))
            {
                return app;
            }

            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();

            return app;
        }

        private static IServiceCollection AddDbConnection(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var connectionString = GetDefaultConnectionString(configuration);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' was not configured."
                );
            }

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString)
            );
            return services;
        }

        private static string? GetDefaultConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return connectionString;
            }

            var database = configuration["POSTGRES_DB"];
            var username = configuration["POSTGRES_USER"];
            var password = configuration["POSTGRES_PASSWORD"];

            if (string.IsNullOrWhiteSpace(database) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var builder = new DbConnectionStringBuilder
            {
                ["Host"] = configuration["POSTGRES_HOST"] ?? "localhost",
                ["Port"] = configuration["POSTGRES_PORT"] ?? "5432",
                ["Database"] = database,
                ["Username"] = username,
                ["Password"] = password,
                ["GSS Encryption Mode"] = "Disable"
            };

            return builder.ConnectionString;
        }
    }
}
