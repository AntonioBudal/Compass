using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Calendar.Domain.Commitments;
using Compass.Modules.Calendar.Domain.Time;

namespace Compass.Modules.Calendar.Application.Commitments.CreateCommitment;

public class CreateCommitmentUseCase
{
    private readonly ICommitmentRepository _repository;

    public CreateCommitmentUseCase(ICommitmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateCommitmentResult> ExecuteAsync(CreateCommitmentCommand command, CancellationToken cancellationToken = default)
    {
        // 1. Tradução do Command para Value Object
        var interval = new TimeInterval(command.Start, command.End);

        // 2. Instanciação do Agregado
        var commitment = new Commitment(command.Title, command.Description, interval);

        // 3. Persistência
        await _repository.AddAsync(commitment, cancellationToken);

        return new CreateCommitmentResult(commitment.Id);
    }
}
