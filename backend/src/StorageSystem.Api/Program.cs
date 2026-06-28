using StorageSystem.Api.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAndConfigureControllers(builder.Configuration);
builder.Services.AddAppConnections(builder.Configuration);
builder.Services.AddUseCases();

var app = builder.Build();

app.ApplyDatabaseMigrations();

app.UseHttpsRedirection();
app.UseDocumentation();

if (builder.Configuration.GetSection("AllowedOrigins").Exists())
{
    app.UseCors("AllowSpecificOrigins");
}

app.MapControllers();

app.Run();
