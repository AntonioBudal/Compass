using System;
using System.Threading.Tasks;
using Compass.Modules.Calendar.Application.Commitments.CreateCommitment;
using Compass.SharedKernel.Domain.Exceptions;
using Xunit;

namespace Compass.Modules.Calendar.Tests.Application.Commitments;

public class CreateCommitmentUseCaseTests
{
    private readonly FakeCommitmentRepository _repository;
    private readonly CreateCommitmentUseCase _useCase;

    public CreateCommitmentUseCaseTests()
    {
        _repository = new FakeCommitmentRepository();
        _useCase = new CreateCommitmentUseCase(_repository);
    }

    [Fact]
    public async Task Should_Create_Commitment_And_Persist()
    {
        var start = DateTimeOffset.UtcNow;
        var command = new CreateCommitmentCommand("Doctor", "Checkup", start, start.AddHours(1));

        var result = await _useCase.ExecuteAsync(command);

        Assert.NotEqual(Guid.Empty, result.CommitmentId);
        
        var saved = Assert.Single(_repository.Saved);
        Assert.Equal("Doctor", saved.Title);
        Assert.Equal("Checkup", saved.Description);
        Assert.Equal(start, saved.Interval.Start);
        Assert.Equal(start.AddHours(1), saved.Interval.End);
    }

    [Fact]
    public async Task Should_Bubble_Up_DomainException_For_Invalid_Interval_Inverted()
    {
        var start = DateTimeOffset.UtcNow;
        var command = new CreateCommitmentCommand("Meeting", null, start, start.AddHours(-1));

        await Assert.ThrowsAsync<DomainException>(async () => await _useCase.ExecuteAsync(command));
        Assert.Empty(_repository.Saved);
    }

    [Fact]
    public async Task Should_Bubble_Up_DomainException_For_Invalid_Interval_Zero_Duration()
    {
        var start = DateTimeOffset.UtcNow;
        var command = new CreateCommitmentCommand("Meeting", null, start, start);

        await Assert.ThrowsAsync<DomainException>(async () => await _useCase.ExecuteAsync(command));
        Assert.Empty(_repository.Saved);
    }

    [Fact]
    public async Task Should_Bubble_Up_DomainException_For_Empty_Title()
    {
        var start = DateTimeOffset.UtcNow;
        var command = new CreateCommitmentCommand("", null, start, start.AddHours(1));

        await Assert.ThrowsAsync<DomainException>(async () => await _useCase.ExecuteAsync(command));
        Assert.Empty(_repository.Saved);
    }
}
