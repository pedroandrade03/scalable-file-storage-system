using StorageSystem.Api.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAndConfigureControllers(builder.Configuration);
builder.Services.AddAppConnections(builder.Configuration);
builder.Services.AddAppStorage(builder.Configuration);
builder.Services.AddAndConfigureDocumentation(builder.Configuration);
builder.Services.AddAndConfigureAuth(builder.Configuration);
builder.Services.AddUseCases();

var app = builder.Build();

app.ApplyDatabaseMigrations();

app.UseDocumentation(builder.Configuration);
app.UseHttpsRedirection();

if (builder.Configuration.GetSection("AllowedOrigins").Exists())
{
    app.UseCors("AllowSpecificOrigins");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
