using Compass.Modules.Calendar.Infrastructure;
using Compass.Modules.Calendar.Presentation.Extensions;
using Compass.Modules.Planning.Infrastructure;
using Compass.Modules.Planning.Presentation.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register Modules
builder.Services.AddCalendarInfrastructure(builder.Configuration);
builder.Services.AddCalendarPresentation();

builder.Services.AddPlanningInfrastructure(builder.Configuration);
builder.Services.AddPlanningPresentation();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

// Map Module Endpoints
app.MapCalendarModuleEndpoints();
app.MapPlanningModuleEndpoints();

app.Run();

public partial class Program { }
