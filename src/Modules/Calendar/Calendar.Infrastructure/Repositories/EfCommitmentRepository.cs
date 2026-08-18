using System;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Calendar.Application.Commitments;
using Compass.Modules.Calendar.Domain.Commitments;
using Compass.Modules.Calendar.Infrastructure.Database;
using Compass.Modules.Calendar.Infrastructure.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Compass.Modules.Calendar.Infrastructure.Repositories;

internal sealed class EfCommitmentRepository : ICommitmentRepository
{
    private readonly CalendarDbContext _dbContext;

    public EfCommitmentRepository(CalendarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Commitment commitment, CancellationToken cancellationToken = default)
    {
        // Nota: Assumimos que o perfil correspondente é gerenciado externamente ou injetado. 
        // Para fins de persistência isolada no módulo, buscamos associar pelo ID do profile se necessário.
        var data = new CommitmentData
        {
            Id = commitment.Id,
            ScheduleProfileId = Guid.Empty, // Ajustaremos conforme o contrato de criação se ligar ao profile
            Title = commitment.Title,
            Description = commitment.Description,
            StartTime = commitment.Interval.Start,
            EndTime = commitment.Interval.End,
            Status = commitment.Status.ToString()
        };

        await _dbContext.Commitments.AddAsync(data, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Commitment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var data = await _dbContext.Commitments.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (data == null) return null;

        var interval = new Domain.Time.TimeInterval(data.StartTime, data.EndTime);
        return new Commitment(data.Title, data.Description, interval);
    }

    public async Task UpdateAsync(Commitment commitment, CancellationToken cancellationToken = default)
    {
        var data = await _dbContext.Commitments.FirstOrDefaultAsync(c => c.Id == commitment.Id, cancellationToken);
        if (data != null)
        {
            data.Title = commitment.Title;
            data.Description = commitment.Description;
            data.StartTime = commitment.Interval.Start;
            data.EndTime = commitment.Interval.End;
            data.Status = commitment.Status.ToString();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
