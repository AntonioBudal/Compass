using System;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;
using Xunit;

namespace Compass.API.IntegrationTests;

public sealed class CompassApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;

    public CompassApiFactory()
    {
        // O Testcontainers sobe um Postgres real no Docker para os testes
        _dbContainer = new PostgreSqlBuilder("postgres:15-alpine")
            
            .WithDatabase("compass_api_e2e_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public string ConnectionString => _dbContainer.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Opcional: Se precisar substituir algum serviço do Host, faça aqui.
            // A ConnectionString já está sendo capturada do appsettings ou variaveis,
            // então forçamos a string do testcontainer via Configuração.
        });

        // Sobrescrevemos a configuração do Host para usar a connection string do Docker
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", ConnectionString);
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}

