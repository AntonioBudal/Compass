using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Calendar.Domain.Profiles;
using Compass.Modules.Calendar.Domain.Time;

namespace Compass.Modules.Calendar.Application.Profiles.UpdateScheduleProfile;

public class UpdateScheduleProfileUseCase
{
    private readonly IScheduleProfileRepository _repository;

    public UpdateScheduleProfileUseCase(IScheduleProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(UpdateScheduleProfileCommand command, CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetByIdAsync(command.ProfileId, cancellationToken);
        if (profile == null)
        {
            throw new Exception($"ScheduleProfile with ID {command.ProfileId} not found.");
        }

        // 1. Atualizar regras diretas do perfil
        profile.SetTimezone(command.Timezone);

        // 2. Tradução: DTO da Application -> Álgebra de Domínio
        var domainSchedule = new Dictionary<DayOfWeek, DaySchedule>();

        foreach (var (day, windowsDto) in command.WeeklySchedule)
        {
            var timeWindows = windowsDto.Select(dto => 
                new TimeWindow(
                    new TimeOfDay(dto.Start.Hours, dto.Start.Minutes),
                    new TimeOfDay(dto.End.Hours, dto.End.Minutes)
                )).ToList();

            // O construtor do DaySchedule validará ordenação e sobreposição
            domainSchedule[day] = new DaySchedule(timeWindows);
        }

        profile.UpdateWeeklySchedule(domainSchedule);

        // 3. Persistência
        await _repository.UpdateAsync(profile, cancellationToken);
    }
}
