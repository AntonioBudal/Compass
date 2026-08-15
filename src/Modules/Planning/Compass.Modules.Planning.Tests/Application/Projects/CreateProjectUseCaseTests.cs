using System;
using Compass.Modules.Planning.Application.Projects.CreateProject;
using Compass.SharedKernel.Domain.Exceptions;
using Xunit;
using ProjectStatus = Compass.Modules.Planning.Domain.Projects.ProjectStatus;

namespace Compass.Modules.Planning.Tests.Application.Projects;

public class CreateProjectUseCaseTests
{
    private readonly FakeProjectRepository _repository;
    private readonly CreateProjectUseCase _useCase;

    public CreateProjectUseCaseTests()
    {
        _repository = new FakeProjectRepository();
        _useCase = new CreateProjectUseCase(_repository);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Create_Project_In_Planning_Status()
    {
        var command = new CreateProjectCommand("Migrate Database");
        var result = await _useCase.ExecuteAsync(command);

        Assert.NotEqual(Guid.Empty, result.ProjectId);
        Assert.Equal(ProjectStatus.Planning, result.Status);

        var savedProject = Assert.Single(_repository.SavedProjects);
        Assert.Equal("Migrate Database", savedProject.Title);
        Assert.Equal(ProjectStatus.Planning, savedProject.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Bubble_Up_Domain_Exception_When_Title_Is_Invalid()
    {
        var command = new CreateProjectCommand(""); // Invariante do domínio
        await Assert.ThrowsAsync<DomainException>(async () => await _useCase.ExecuteAsync(command));
        Assert.Empty(_repository.SavedProjects);
    }
}
