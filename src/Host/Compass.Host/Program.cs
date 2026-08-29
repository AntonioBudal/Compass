using Compass.Modules.Calendar.Infrastructure;
using Compass.Modules.Calendar.Presentation.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register Modules
builder.Services.AddCalendarInfrastructure(builder.Configuration);
builder.Services.AddCalendarPresentation();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

// Map Module Endpoints
app.MapCalendarModuleEndpoints();

app.Run();

public partial class Program { }
