using Compass.Domain.Entities;
using Compass.Domain.Enums;

namespace Compass.Tests.Shared.Builders;

public class TaskCommitmentBuilder
{
    private Guid _userId = TestConstants.DefaultUserId;
    private string _title = "Tarefa Padrão de Teste";
    private int _duration = 30;
    private short _energy = 2;
    private Guid? _projectId = null;
    private DateTime? _deadline = null;
    private CommitmentStatus _status = CommitmentStatus.Pending;

    public TaskCommitmentBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public TaskCommitmentBuilder WithDuration(int duration)
    {
        _duration = duration;
        return this;
    }

    public TaskCommitmentBuilder WithEnergy(short energy)
    {
        _energy = energy;
        return this;
    }

    public TaskCommitmentBuilder WithStatus(CommitmentStatus status)
    {
        _status = status;
        return this;
    }

    public TaskCommitment Build()
    {
        var task = new TaskCommitment(_userId, _title, _duration, _energy, _projectId, _deadline);
        
        // Simula o comportamento real de mudança de estado da entidade
        if (_status == CommitmentStatus.InProgress) task.StartFocus();
        if (_status == CommitmentStatus.Completed) task.Complete();
        if (_status == CommitmentStatus.Blocked) task.Block();
        if (_status == CommitmentStatus.Archived) task.Archive();

        return task;
    }
}