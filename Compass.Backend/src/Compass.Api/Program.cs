using Compass.Api.Middlewares;
using Compass.Application.Interfaces;
using Compass.Application.Services;
using Compass.Application.Validators;
using Compass.Domain.Interfaces;
using Compass.Infrastructure.Persistence;
using Compass.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

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

app.Run();