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

var builder = WebApplication.CreateBuilder(args);

// 1. Conexão com o PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");

builder.Services.AddDbContext<CompassDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly("Compass.Infrastructure");
    });
});

// 2. Registro do Tratamento Global de Erros (RFC 7807)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 3. Registro dos Repositórios (Infraestrutura)
builder.Services.AddScoped<ICommitmentRepository, CommitmentRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IDecisionSnapshotRepository, DecisionSnapshotRepository>();

// 4. Registro dos Serviços (Aplicação)
builder.Services.AddScoped<IDecisionService, DecisionService>();
builder.Services.AddScoped<ICommitmentService, CommitmentService>();

// 5. Registro dos Validadores do FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateCommitmentDtoValidator>();

// 6. Configuração de CORS para o Frontend (Vue.js 3 / Vite)
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueFrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // Portas padrão do Vite/Vue
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks().AddDbContextCheck<CompassDbContext>("postgres_db", tags: new[] { "db", "sql", "postgresql" });

var app = builder.Build();

// Interceptador Global de Erros deve vir antes do mapeamento de rotas
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
    var db = scope.ServiceProvider.GetRequiredService<Compass.Infrastructure.Persistence.CompassDbContext>();
    
    // Insere o usuário direto via SQL ignorando as entidades do C#!
    db.Database.ExecuteSqlRaw(@"
        INSERT INTO users (id, name, email, password_hash) 
        VALUES ('11111111-1111-1111-1111-111111111111', 'Operador Local', 'local@compass.dev', 'hash') 
        ON CONFLICT (id) DO NOTHING;
    ");
}

app.MapHealthChecks("/api/v1/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            durationMs = report.TotalDuration.TotalMilliseconds,
            entries = report.Entries.Select(e => new
            {
                component = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description ?? "OK",
                latencyMs = e.Value.Duration.TotalMilliseconds
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.Run();