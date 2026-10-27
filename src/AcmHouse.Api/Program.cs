using System.Text.Json.Serialization;
using AcmHouse.Api.Extensions;
using AcmHouse.Api.Middleware;
using AcmHouse.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddApplication();

// Integration tests run in the "Testing" environment and plug in their own database.
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddRepositories();
}
else
{
    string connectionString = builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Connection string 'Default' is not set.");
    builder.Services.AddInfrastructure(connectionString);
}

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerWithJwt();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

await app.InitializeDatabaseAsync();

app.Run();

// Lets WebApplicationFactory<Program> in the integration tests find the entry point.
public partial class Program { }
