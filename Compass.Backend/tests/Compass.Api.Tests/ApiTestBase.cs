using Compass.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Compass.Api.Tests;

public abstract class ApiTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly HttpClient Client;
    protected readonly CustomWebApplicationFactory Factory;

    protected ApiTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        
        // Cria um cliente HTTP que se comunica diretamente com o servidor em memória (sem portas de rede reais)
        Client = factory.CreateClient();
    }

    // Utilitário para limpar o banco em memória entre um teste e outro
    protected void ResetDatabase()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CompassDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }
}