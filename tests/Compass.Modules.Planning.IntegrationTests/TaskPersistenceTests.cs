using Compass.Modules.Planning.Domain.Model;
using Compass.Modules.Planning.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using TaskModel = Compass.Modules.Planning.Domain.Model.Task;
using TaskStatus = Compass.Modules.Planning.Domain.Model.TaskStatus;

namespace Compass.Modules.Planning.IntegrationTests;

public class TaskPersistenceTests : IClassFixture<PlanningTestDatabaseFixture>
{
    private readonly PlanningTestDatabaseFixture _fixture;

    public TaskPersistenceTests(PlanningTestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAndRetrieveTask_ShouldPersistInPostgresPlanningSchema()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        var repo = new TaskRepository(context);
        var task = TaskModel.Create(
            "Configurar Dockerfile",
            "Criar multistage build",
            durationMinutes: 45,
            deadline: DateTimeOffset.UtcNow.AddDays(2)
        );

        // Act
        await repo.AddAsync(task);
        await repo.SaveChangesAsync();

        // Assert
        await using var verifyContext = _fixture.CreateDbContext();
        var verifyRepo = new TaskRepository(verifyContext);
        var retrieved = await verifyRepo.GetByIdAsync(task.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(task.Id);
        retrieved.Title.Should().Be("Configurar Dockerfile");
        retrieved.Description.Should().Be("Criar multistage build");
        retrieved.DurationMinutes.Should().Be(45);
        retrieved.Status.Should().Be(TaskStatus.Ready);
    }
}
