using Compass.Modules.Execution.Application.DailyCycles.RecordExecution;
using Compass.Modules.Execution.Presentation;
using Compass.Modules.Execution;
using Compass.API.Middleware;
using Compass.Modules.Execution.Infrastructure;
using Compass.Modules.Planning;
using Compass.Modules.Planning.Infrastructure;
using Compass.Modules.Planning.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=compass_dev;Username=postgres;Password=postgres";

builder.Services.AddPlanningApplication();
builder.Services.AddExecutionApplication();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        typeof(RecordExecutionCommandHandler).Assembly,
        typeof(PlanningEndpoints).Assembly,
        typeof(Compass.Modules.Planning.Infrastructure.DependencyInjection).Assembly
    );
});
builder.Services.AddPlanningInfrastructure(connectionString);

builder.Services.AddExecutionInfrastructure(connectionString);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapPlanningEndpoints();
app.MapExecutionEndpoints();

// Migrations
using (var scope = app.Services.CreateScope())
{
    var planningDb = scope.ServiceProvider.GetService<
        Compass.Modules.Planning.Infrastructure.Database.PlanningDbContext>();

    if (planningDb != null)
    {
        await Microsoft.EntityFrameworkCore
            .RelationalDatabaseFacadeExtensions
            .MigrateAsync(planningDb.Database);
    }
}

await app.Services.MigrateExecutionDatabaseAsync();

app.Run();

public partial class Program { }


