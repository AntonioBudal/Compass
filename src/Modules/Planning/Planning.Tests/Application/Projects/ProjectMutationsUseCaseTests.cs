using System;
using Compass.Modules.Planning.Application.Projects.ActivateProject;
using Compass.Modules.Planning.Application.Projects.CompleteProject;
using Compass.Modules.Planning.Application.Projects.PauseProject;
using Compass.SharedKernel.Domain.Exceptions;
using Xunit;
using Project = Compass.Modules.Planning.Domain.Projects.Project;
using ProjectStatus = Compass.Modules.Planning.Domain.Projects.ProjectStatus;

namespace Compass.Modules.Planning.Tests.Application.Projects;

public class ProjectMutationsUseCaseTests
{
    private readonly FakeProjectRepository _repository;

    public ProjectMutationsUseCaseTests()
    {
        _repository = new FakeProjectRepository();
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Activate_Planning_Project()
    {
        var project = new Project("Init"); // Inicia como Planning
        await _repository.AddAsync(project);
        
        var useCase = new ActivateProjectUseCase(_repository);
        await useCase.ExecuteAsync(new ActivateProjectCommand(project.Id));

        var updated = await _repository.GetByIdAsync(project.Id);
        Assert.Equal(ProjectStatus.Active, updated!.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Pause_Active_Project()
    {
        var project = new Project("Doing");
        project.Activate();
        await _repository.AddAsync(project);
        
        var useCase = new PauseProjectUseCase(_repository);
        await useCase.ExecuteAsync(new PauseProjectCommand(project.Id));

        var updated = await _repository.GetByIdAsync(project.Id);
        Assert.Equal(ProjectStatus.Paused, updated!.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Complete_Active_Project()
    {
        var project = new Project("Finishing");
        project.Activate();
        await _repository.AddAsync(project);
        
        var useCase = new CompleteProjectUseCase(_repository);
        await useCase.ExecuteAsync(new CompleteProjectCommand(project.Id));

        var updated = await _repository.GetByIdAsync(project.Id);
        Assert.Equal(ProjectStatus.Completed, updated!.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Bubble_Up_DomainException_If_Pausing_Planning_Project()
    {
        var project = new Project("Planning");
        await _repository.AddAsync(project);
        
        var useCase = new PauseProjectUseCase(_repository);
        
        // Domínio não permite pausar o que ainda está em planejamento
        await Assert.ThrowsAsync<DomainException>(async () => await useCase.ExecuteAsync(new PauseProjectCommand(project.Id)));
    }
}
