using Compass.API.Middleware;
using Compass.Modules.Planning;
using Compass.Modules.Planning.Infrastructure;
using Compass.Modules.Planning.Presentation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Exception Handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Configura a Connection String. 
// OBS: Em desenvolvimento local apontaremos para o compass_dev criado.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=compass_dev;Username=postgres;Password=postgres";

// Registrar os Módulos (Nossa "Costura")
builder.Services.AddPlanningApplication();
builder.Services.AddPlanningInfrastructure(connectionString);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler(); // Ativa o GlobalExceptionHandler
app.UseHttpsRedirection();

// Mapear Endpoints dos Módulos
app.MapPlanningEndpoints();

app.Run();

public partial class Program { }
