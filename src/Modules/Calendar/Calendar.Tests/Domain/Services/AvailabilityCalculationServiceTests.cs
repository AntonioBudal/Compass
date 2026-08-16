using System;
using System.Collections.Generic;
using Compass.Modules.Calendar.Domain.Commitments;
using Compass.Modules.Calendar.Domain.Profiles;
using Compass.Modules.Calendar.Domain.Services;
using Compass.Modules.Calendar.Domain.Time;
using Xunit;

namespace Compass.Modules.Calendar.Tests.Domain.Services;

public class AvailabilityCalculationServiceTests
{
    private static DateTimeOffset T(int hour, int minute = 0) => new DateTimeOffset(2026, 8, 18, hour, minute, 0, TimeSpan.Zero); // Terça-feira (UTC)

    private ScheduleProfile CreateProfile(string timezone = "UTC")
    {
        var profile = new ScheduleProfile(Guid.NewGuid(), timezone);
        var weekly = new Dictionary<DayOfWeek, DaySchedule>
        {
            { DayOfWeek.Tuesday, new DaySchedule(new[] { new TimeWindow(new TimeOfDay(8, 0), new TimeOfDay(12, 0)) }) }
        };
        profile.UpdateWeeklySchedule(weekly);
        return profile;
    }

    [Fact]
    public void Calculate_CleanSchedule_ShouldProjectCorrectly()
    {
        var service = new AvailabilityCalculationService();
        var profile = CreateProfile();
        var query = new TimeInterval(T(0), T(23));

        var windows = service.Calculate(profile, Array.Empty<Commitment>(), query, TimeSpan.FromMinutes(15));

        var window = Assert.Single(windows);
        Assert.Equal(T(8), window.Interval.Start);
        Assert.Equal(T(12), window.Interval.End);
        Assert.Equal(TimeSpan.FromHours(4), window.Duration);
    }

    [Fact]
    public void Calculate_WithDoubleBooking_ShouldMergeAndSubtract()
    {
        var service = new AvailabilityCalculationService();
        var profile = CreateProfile();
        var query = new TimeInterval(T(0), T(23));

        var commitments = new[]
        {
            new Commitment("Meeting 1", null, new TimeInterval(T(9), T(10))),
            new Commitment("Meeting 2", null, new TimeInterval(T(9, 30), T(11)))
        }; 

        var windows = service.Calculate(profile, commitments, query, TimeSpan.FromMinutes(15));

        Assert.Equal(2, windows.Count);
        Assert.Equal(T(8), windows[0].Interval.Start);
        Assert.Equal(T(9), windows[0].Interval.End);
        
        Assert.Equal(T(11), windows[1].Interval.Start);
        Assert.Equal(T(12), windows[1].Interval.End);
    }

    [Fact]
    public void Calculate_ShouldPrune_WindowsSmallerThanMinimumDuration()
    {
        var service = new AvailabilityCalculationService();
        var profile = CreateProfile(); // Tem turno [08:00 - 12:00]
        var query = new TimeInterval(T(0), T(23));

        var commitments = new[]
        {
            // Fura a agenda, deixando um buraco de apenas 10 minutos (11:50 - 12:00)
            new Commitment("Long Meeting", null, new TimeInterval(T(8), T(11, 50)))
        };

        // Act: Definimos que o foco mínimo para gerar trabalho é de 15 minutos
        var windows = service.Calculate(profile, commitments, query, TimeSpan.FromMinutes(15));

        // Assert: A janela de 10 minutos que sobrou foi descartada (Podada)
        Assert.Empty(windows);
    }
}
