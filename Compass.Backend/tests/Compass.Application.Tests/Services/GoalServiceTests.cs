using Compass.Application.DTOs;
using Compass.Application.Services;
using Compass.Domain.Entities;
using Compass.Domain.Exceptions;
using Compass.Domain.Interfaces;
using Compass.Tests.Shared;
using Compass.Tests.Shared.Builders;
using FluentAssertions;
using Moq;

namespace Compass.Application.Tests.Services;

public class GoalServiceTests
{
    private readonly Mock<IGoalRepository> _goalRepositoryMock;
    private readonly GoalService _sut; // SUT = System Under Test

    public GoalServiceTests()
    {
        // Instanciamos o Mock estrito para a interface
        _goalRepositoryMock = new Mock<IGoalRepository>();
        
        // Injetamos o mock no serviço real
        _sut = new GoalService(_goalRepositoryMock.Object);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowDomainException_WhenGoalDoesNotExist()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        _goalRepositoryMock.Setup(repo => repo.GetByIdAsync(goalId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync((Goal?)null); // Simula o banco não encontrando a meta

        var dto = new UpdateGoalDto("Título Válido", null, null);

        // Act
        Func<Task> action = async () => await _sut.UpdateAsync(TestConstants.DefaultUserId, goalId, dto);

        // Assert
        await action.Should().ThrowAsync<DomainException>()
                    .WithMessage("*não encontrada*");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowDomainException_WhenUserIsUnauthorized()
    {
        // Arrange
        var unauthorizedUserId = Guid.NewGuid(); // Outro usuário
        var goal = new GoalBuilder().WithUserId(unauthorizedUserId).Build(); 
        
        _goalRepositoryMock.Setup(repo => repo.GetByIdAsync(goal.Id, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(goal);

        var dto = new UpdateGoalDto("Novo Título", null, null);

        // Act
        // Tentamos atualizar passando o ID do usuário logado (que é diferente do dono da meta)
        Func<Task> action = async () => await _sut.UpdateAsync(TestConstants.DefaultUserId, goal.Id, dto);

        // Assert
        await action.Should().ThrowAsync<DomainException>()
                    .WithMessage("*Acesso negado*");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateAndSaveChanges_WhenRequestIsValid()
    {
        // Arrange
        var goal = new GoalBuilder()
            .WithUserId(TestConstants.DefaultUserId)
            .WithTitle("Título Antigo")
            .Build();
            
        _goalRepositoryMock.Setup(repo => repo.GetByIdAsync(goal.Id, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(goal);

        var dto = new UpdateGoalDto("Título Atualizado", "Nova descrição", null);

        // Act
        var result = await _sut.UpdateAsync(TestConstants.DefaultUserId, goal.Id, dto);

        // Assert
        goal.Title.Should().Be("Título Atualizado");
        
        // Verifica se o serviço realmente repassou o objeto mutado para o repositório
        _goalRepositoryMock.Verify(repo => repo.Update(goal), Times.Once);
        
        // Verifica se a transação do banco foi comitada
        _goalRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        result.Should().NotBeNull();
    }
}