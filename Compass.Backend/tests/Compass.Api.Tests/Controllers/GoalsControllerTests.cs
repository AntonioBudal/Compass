using System.Net;
using System.Net.Http.Json;
using Compass.Application.DTOs;
using Compass.Infrastructure.Persistence;
using Compass.Tests.Shared;
using Compass.Tests.Shared.Builders;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Compass.Api.Tests.Controllers;

public class GoalsControllerTests : ApiTestBase
{
    public GoalsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Update_ShouldReturnOkAndPersistChanges_WhenRequestIsValid()
    {
        // Arrange
        ResetDatabase(); // Garante um banco limpo antes do teste

        // 1. Instancia a meta via Builder e "semeia" no banco em memória
        var goal = new GoalBuilder()
            .WithUserId(TestConstants.DefaultUserId)
            .WithTitle("Meta Antiga")
            .Build();

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CompassDbContext>();
            db.Goals.Add(goal);
            await db.SaveChangesAsync();
        }

        // 2. Monta o Payload DTO (Exatamente como o Pinia/Axios faria)
        var payload = new UpdateGoalDto("Nova Meta Testada", "Descrição atualizada", DateTime.UtcNow.AddDays(5));

        // Injeta o Header que o nosso ProjectsController/GoalsController extrai
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", TestConstants.DefaultUserId.ToString());

        // Act (Dispara a requisição HTTP PUT para o Endpoint)
        var response = await Client.PutAsJsonAsync($"/api/v1/goals/{goal.Id}", payload);

        // Assert (1): A API deve responder 200 OK
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert (2): Inspecionamos o banco de dados para garantir o "Não-Phantom Save"
        using (var validationScope = Factory.Services.CreateScope())
        {
            var db = validationScope.ServiceProvider.GetRequiredService<CompassDbContext>();
            var updatedGoal = await db.Goals.FindAsync(goal.Id);
            
            updatedGoal.Should().NotBeNull();
            updatedGoal!.Title.Should().Be("Nova Meta Testada");
            updatedGoal.WhyDescription.Should().Be("Descrição atualizada");
        }
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenTitleIsInvalid()
    {
        // Arrange
        ResetDatabase();
        var goal = new GoalBuilder().Build();
        
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CompassDbContext>();
            db.Goals.Add(goal);
            await db.SaveChangesAsync();
        }

        // Payload com falha intencional (2 caracteres vai ser bloqueado pelo FluentValidation)
        var payload = new UpdateGoalDto("AB", null, null);
        
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", TestConstants.DefaultUserId.ToString());

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/goals/{goal.Id}", payload);

        // Assert: Valida se o Middleware de Exceções ou o Validador estão barrando a chamada
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}