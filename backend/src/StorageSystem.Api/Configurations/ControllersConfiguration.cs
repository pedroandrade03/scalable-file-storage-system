using StorageSystem.Api.Filters;

namespace StorageSystem.Api.Configurations
{
    public static class ControllersConfiguration
    {
        public static IServiceCollection AddAndConfigureControllers(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddControllers(options =>
            {
                options.Filters.Add<ApiGlobalExceptionFilter>();
            });
            services.AddProblemDetails();
            services.AddDocumentation();
            services.AddAndConfigureCors(configuration);
            return services;
        }

        private static IServiceCollection AddDocumentation(
            this IServiceCollection services
        )
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            return services;
        }

        public static WebApplication UseDocumentation(
            this WebApplication app
        )
        {
            if (app.Environment.IsDevelopment())
            {   
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            return app;
        }

        private static IServiceCollection AddAndConfigureCors(
            this IServiceCollection services, 
            IConfiguration configuration
        )
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
}
