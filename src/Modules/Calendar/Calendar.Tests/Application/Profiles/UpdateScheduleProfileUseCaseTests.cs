using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Compass.Modules.Calendar.Application.Profiles.UpdateScheduleProfile;
using Compass.Modules.Calendar.Domain.Profiles;
using Compass.SharedKernel.Domain.Exceptions;
using Xunit;

namespace Compass.Modules.Calendar.Tests.Application.Profiles;

public class UpdateScheduleProfileUseCaseTests
{
    private readonly FakeScheduleProfileRepository _repository;
    private readonly UpdateScheduleProfileUseCase _useCase;
    private readonly Guid _profileId;

    public UpdateScheduleProfileUseCaseTests()
    {
        _repository = new FakeScheduleProfileRepository();
        _useCase = new UpdateScheduleProfileUseCase(_repository);
        _profileId = Guid.NewGuid();
        
        // Arrange base: Seed profile
        _repository.Saved.Add(new ScheduleProfile(_profileId, "UTC"));
    }

    [Fact]
    public async Task Should_Update_Profile_With_Valid_Timezone_And_Two_Windows_Same_Day()
    {
        var command = new UpdateScheduleProfileCommand(
            _profileId,
            "UTC",
            new Dictionary<DayOfWeek, IEnumerable<TimeWindowDto>>
            {
                { DayOfWeek.Monday, new[] 
                    { 
                        new TimeWindowDto(new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0)),
                        new TimeWindowDto(new TimeSpan(14, 0, 0), new TimeSpan(18, 0, 0)) 
                    } 
                }
            }
        );

        await _useCase.ExecuteAsync(command);

        var profile = await _repository.GetByIdAsync(_profileId);
        Assert.Equal("UTC", profile!.Timezone);
        
        var monday = profile.WeeklySchedule[DayOfWeek.Monday];
        Assert.Equal(2, monday.Windows.Count);
        
        // Verifica ordenação indiretamente (08h vem antes de 14h)
        Assert.Equal(8, monday.Windows[0].Start.Hour);
        Assert.Equal(14, monday.Windows[1].Start.Hour);
    }

    [Fact]
    public async Task Should_Bubble_Up_DomainException_For_Overlapping_Windows()
    {
        var command = new UpdateScheduleProfileCommand(
            _profileId,
            "UTC",
            new Dictionary<DayOfWeek, IEnumerable<TimeWindowDto>>
            {
                { DayOfWeek.Monday, new[] 
                    { 
                        new TimeWindowDto(new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0)),
                        new TimeWindowDto(new TimeSpan(12, 0, 0), new TimeSpan(16, 0, 0)) // Sobrepõe
                    } 
                }
            }
        );

        await Assert.ThrowsAsync<DomainException>(async () => await _useCase.ExecuteAsync(command));
    }

    [Fact]
    public async Task Should_Bubble_Up_DomainException_For_Inverted_Interval()
    {
        var command = new UpdateScheduleProfileCommand(
            _profileId,
            "UTC",
            new Dictionary<DayOfWeek, IEnumerable<TimeWindowDto>>
            {
                { DayOfWeek.Tuesday, new[] 
                    { 
                        new TimeWindowDto(new TimeSpan(18, 0, 0), new TimeSpan(14, 0, 0)) // Invertido
                    } 
                }
            }
        );

        await Assert.ThrowsAsync<DomainException>(async () => await _useCase.ExecuteAsync(command));
    }

    [Fact]
    public async Task Should_Bubble_Up_DomainException_For_Invalid_Timezone()
    {
        var command = new UpdateScheduleProfileCommand(
            _profileId,
            "Invalid/Timezone_Name",
            new Dictionary<DayOfWeek, IEnumerable<TimeWindowDto>>()
        );

        await Assert.ThrowsAsync<DomainException>(async () => await _useCase.ExecuteAsync(command));
    }

    [Fact]
    public async Task Should_Throw_Exception_When_Profile_Not_Found()
    {
        var command = new UpdateScheduleProfileCommand(
            Guid.NewGuid(), // Inexistente
            "UTC",
            new Dictionary<DayOfWeek, IEnumerable<TimeWindowDto>>()
        );

        var ex = await Assert.ThrowsAsync<Exception>(async () => await _useCase.ExecuteAsync(command));
        Assert.Contains("not found", ex.Message);
    }
}
