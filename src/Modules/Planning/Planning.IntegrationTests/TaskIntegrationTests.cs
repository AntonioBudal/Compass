using System;
using System.Net;
using System.Net.Http.Json;
using Compass.Modules.Planning.Application.Tasks.CreateTask;
using Compass.Modules.Planning.Infrastructure.Database;
using Compass.Modules.Planning.IntegrationTests.Setup;
using Compass.Modules.Planning.Presentation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using TaskStatus = Compass.Modules.Planning.Domain.Tasks.TaskStatus;

namespace Compass.Modules.Planning.IntegrationTests;

[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<PlanningApiFactory> { }

[Collection("Integration")]
public class TaskIntegrationTests
{
    private readonly PlanningApiFactory _factory;
    private readonly System.Net.Http.HttpClient _client;

    public TaskIntegrationTests(PlanningApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async System.Threading.Tasks.Task Test1_Should_Create_Task_And_Persist_To_Database()
    {
        // 1. Arrange: Envia a requisição HTTP
        var command = new CreateTaskCommand("Integration Task");

        // 2. Act: Atravessa API -> UseCase -> Domínio -> Banco de Dados
        var response = await _client.PostAsJsonAsync("/api/planning/tasks", command);

        // 3. Assert HTTP: Respondeu 200 OK com o ID
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CreateTaskResult>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.TaskId);
        Assert.Equal(TaskStatus.Draft, result.Status);

        // 4. Assert Banco de Dados Real: Confirma que o EF Core persistiu
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PlanningDbContext>();
        
        var savedTask = await dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == result.TaskId);
        Assert.NotNull(savedTask);
        Assert.Equal("Integration Task", savedTask.Title);
        Assert.Equal(TaskStatus.Draft, savedTask.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task Test2_Should_Estimate_Task_And_Update_Status_To_Ready()
    {
        // Arrange: Cria a task pela API primeiro
        var createCommand = new CreateTaskCommand("Task to Estimate");
        var createResponse = await _client.PostAsJsonAsync("/api/planning/tasks", createCommand);
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateTaskResult>();
        var taskId = createResult!.TaskId;

        var estimateRequest = new EstimateTaskRequest(45);

        // Act: Modifica a tarefa
        var estimateResponse = await _client.PutAsJsonAsync($"/api/planning/tasks/{taskId}/estimate", estimateRequest);

        // Assert HTTP
        Assert.Equal(HttpStatusCode.NoContent, estimateResponse.StatusCode);

        // Assert Banco: Lê do banco para provar o estado do domínio
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PlanningDbContext>();
        
        var updatedTask = await dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
        Assert.NotNull(updatedTask);
        Assert.Equal(45, updatedTask.EstimatedDurationMinutes);
        Assert.Equal(TaskStatus.Ready, updatedTask.Status); // Comprova a mutação de estado!
    }

    [Fact]
    public async System.Threading.Tasks.Task Test3_Should_Return_BadRequest_When_Starting_Draft_Task()
    {
        // Arrange: Cria a task pela API (nasce Draft)
        var createCommand = new CreateTaskCommand("Draft Task to Fail");
        var createResponse = await _client.PostAsJsonAsync("/api/planning/tasks", createCommand);
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateTaskResult>();
        var taskId = createResult!.TaskId;

        // Act: Tenta iniciar diretamente (vai estourar no domínio)
        var startResponse = await _client.PutAsync($"/api/planning/tasks/{taskId}/start", null);

        // Assert Middleware HTTP: Garante que a exceção não quebrou o app (500), mas virou um 400 estruturado
        Assert.Equal(HttpStatusCode.BadRequest, startResponse.StatusCode);
        
        var problemDetails = await startResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal("Domain Rule Violation", problemDetails.Title);
        Assert.Contains("Estimate time first", problemDetails.Detail);
    }
}
