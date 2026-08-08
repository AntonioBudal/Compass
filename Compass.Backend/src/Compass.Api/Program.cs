using System.Text.Json;
using Compass.Api.Middlewares;
using Compass.Application.Interfaces;
using Compass.Application.Services;
using Compass.Application.Validators;
using Compass.Domain.Interfaces;
using Compass.Infrastructure.Persistence;
using Compass.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Compass.Infrastructure.Services;
using Compass.Api.Workers;

var builder = WebApplication.CreateBuilder(args);

// 1. Conexão com o Banco de Dados (SQLite - Banco Local)
builder.Services.AddDbContext<CompassDbContext>(options =>
{
    // Só injeta o motor do SQLite se NÃO for o ambiente de testes
    if (!builder.Environment.IsEnvironment("Testing"))
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");

        //  ARQ: A Mágica da Abstração. O EF Core agora traduzirá tudo para SQLite.
        options.UseSqlite(connectionString);
    }

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// 2. Registro do Tratamento Global de Erros (RFC 7807)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 3. Registro dos Repositórios (Infraestrutura)
builder.Services.AddScoped<ICommitmentRepository, CommitmentRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IGoalRepository, GoalRepository>();
builder.Services.AddScoped<IDecisionSnapshotRepository, DecisionSnapshotRepository>();

// 4. Registro dos Serviços (Aplicação)
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IDecisionService, DecisionService>();
builder.Services.AddScoped<ICommitmentService, CommitmentService>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<IUserBehaviorProfilerService, UserBehaviorProfilerService>();
builder.Services.AddScoped<IDataPortabilityService, DataPortabilityService>();
builder.Services.AddScoped<IDailyCycleService, DailyCycleService>();
builder.Services.AddHostedService<BehavioralCalibrationWorker>();

// 5. Registro dos Validadores do FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateCommitmentDtoValidator>();

// 6. Configuração de CORS para o Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueFrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Ajuste do Health Check para SQLite
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CompassDbContext>("sqlite_db", tags: new[] { "db", "sql", "sqlite" });

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("VueFrontendPolicy");
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CompassDbContext>();
    
    // O script SQL roda APENAS se NÃO estivermos no ambiente de testes
    if (!app.Environment.IsEnvironment("Testing"))
    {
        // Dialeto SQLite para evitar duplicação do Operador Local
        db.Database.ExecuteSqlRaw(@"
            INSERT OR IGNORE INTO users (id, name, email, password_hash, created_at, updated_at) 
            VALUES ('11111111-1111-1111-1111-111111111111', 'Operador Local', 'local@compass.dev', 'hash', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
        ");
    }
}

app.MapHealthChecks("/api/v1/healthz", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            timestamp = DateTime.UtcNow,
            dependencies = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                durationMs = e.Value.Duration.TotalMilliseconds
            })
        };

        await JsonSerializer.SerializeAsync(context.Response.Body, response);
    }
});

app.Run();

public partial class Program { }