using System.Data.Common;
using System.Linq;
using Compass.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Compass.Api.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // 1. A VARREDURA LETAL: Encontra TODOS os serviços que o EF Core e o Npgsql registraram silenciosamente
            var efCoreServices = services
                .Where(d => d.ServiceType.Namespace != null && 
                           (d.ServiceType.Namespace.StartsWith("Microsoft.EntityFrameworkCore") || 
                            d.ServiceType.Namespace.StartsWith("Npgsql")))
                .ToList();

            // Arranca todos eles da Injeção de Dependências
            foreach (var descriptor in efCoreServices)
            {
                services.Remove(descriptor);
            }

            // 2. Arranca a conexão física de banco de dados (se houver alguma solta)
            var dbConnectionDescriptors = services
                .Where(d => d.ServiceType == typeof(DbConnection) || d.ServiceType.IsSubclassOf(typeof(DbConnection)))
                .ToList();

            foreach (var descriptor in dbConnectionDescriptors)
            {
                services.Remove(descriptor);
            }

            // 3. Arranca o próprio CompassDbContext e suas Opções (que estão no nosso namespace, não no da Microsoft)
            var dbContextOptions = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<CompassDbContext>));
            if (dbContextOptions != null) services.Remove(dbContextOptions);

            var dbContext = services.SingleOrDefault(d => d.ServiceType == typeof(CompassDbContext));
            if (dbContext != null) services.Remove(dbContext);

            // 4. AGORA SIM: O terreno está completamente limpo. Injetamos o banco em memória isolado.
            services.AddDbContext<CompassDbContext>(options =>
            {
                options.UseInMemoryDatabase("CompassApiTestDb");
            });
        });
    }
}