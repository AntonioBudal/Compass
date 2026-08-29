using Compass.Modules.Calendar.Domain.Model;
using Compass.Modules.Calendar.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Compass.Modules.Calendar.IntegrationTests;

public class ScheduleProfilePersistenceTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public ScheduleProfilePersistenceTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAndGetById_ShouldPersistAndRetrieveScheduleProfile()
    {
        // Arrange
        using (var initContext = _fixture.CreateDbContext())
        {
            await initContext.Database.EnsureCreatedAsync();
        }

        var timeZone = new TimeZoneId("America/Sao_Paulo");
        var window1 = new TimeWindow(new TimeOnly(9, 0), new TimeOnly(12, 0));
        var window2 = new TimeWindow(new TimeOnly(13, 0), new TimeOnly(18, 0));
        var ruleMonday = new DayAvailabilityRule(DayOfWeek.Monday, [window1, window2]);

        var profile = ScheduleProfile.Create(timeZone, [ruleMonday]);

        // Act - Save
        using (var saveContext = _fixture.CreateDbContext())
        {
            var repo = new ScheduleProfileRepository(saveContext);
            await repo.AddAsync(profile);
            await repo.SaveChangesAsync();
        }

        // Assert - Read from clean context
        using (var readContext = _fixture.CreateDbContext())
        {
            var repo = new ScheduleProfileRepository(readContext);
            var retrieved = await repo.GetByIdAsync(profile.Id);

            retrieved.Should().NotBeNull();
            retrieved!.Id.Should().Be(profile.Id);
            retrieved.TimeZone.Value.Should().Be("America/Sao_Paulo");
            retrieved.WeeklyAvailability.Should().HaveCount(1);

            var readRule = retrieved.WeeklyAvailability.First();
            readRule.DayOfWeek.Should().Be(DayOfWeek.Monday);
            readRule.Windows.Should().HaveCount(2);
            readRule.Windows[0].StartTime.Should().Be(new TimeOnly(9, 0));
            readRule.Windows[0].EndTime.Should().Be(new TimeOnly(12, 0));
            readRule.Windows[1].StartTime.Should().Be(new TimeOnly(13, 0));
            readRule.Windows[1].EndTime.Should().Be(new TimeOnly(18, 0));
        }
    }
}
