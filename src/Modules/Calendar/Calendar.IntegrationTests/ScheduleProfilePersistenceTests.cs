using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Compass.Modules.Calendar.Domain.Profiles;
using Compass.Modules.Calendar.Domain.Time;
using Compass.Modules.Calendar.Infrastructure.Database;
using Compass.Modules.Calendar.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Compass.Modules.Calendar.IntegrationTests;

public class ScheduleProfilePersistenceTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("compass_calendar_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private CalendarDbContext _dbContext = null!;
    private EfScheduleProfileRepository _repository = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var options = new DbContextOptionsBuilder<CalendarDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        _dbContext = new CalendarDbContext(options);
        await _dbContext.Database.MigrateAsync();

        _repository = new EfScheduleProfileRepository(_dbContext);
    }

    [Fact]
    public async Task Should_Persist_And_Reconstruct_ScheduleProfile_Without_Data_Loss()
    {
        // 1. Arrange: Montar um Domínio super complexo
        var profileId = Guid.NewGuid();
        var originalProfile = new ScheduleProfile(profileId, "America/Sao_Paulo");

        var domainSchedule = new Dictionary<DayOfWeek, DaySchedule>
        {
            { DayOfWeek.Monday, new DaySchedule(new[] {
                new TimeWindow(new TimeOfDay(8, 30), new TimeOfDay(12, 0)),
                new TimeWindow(new TimeOfDay(13, 15), new TimeOfDay(18, 45))
            })},
            { DayOfWeek.Friday, new DaySchedule(new[] {
                new TimeWindow(new TimeOfDay(9, 0), new TimeOfDay(14, 0))
            })}
        };
        originalProfile.UpdateWeeklySchedule(domainSchedule);

        // 2. Act: Persistir no banco relacional via EF Core
        await _repository.AddAsync(originalProfile);
        _dbContext.ChangeTracker.Clear(); // Limpa cache para forçar query real

        // 3. Act: Recuperar do banco e reconstruir
        var reconstructedProfile = await _repository.GetByIdAsync(profileId);

        // 4. Assert: Provar que NADA se perdeu na tradução
        Assert.NotNull(reconstructedProfile);
        Assert.Equal(profileId, reconstructedProfile.Id);
        Assert.Equal("America/Sao_Paulo", reconstructedProfile.Timezone);
        Assert.Equal(2, reconstructedProfile.WeeklySchedule.Count);

        // Validar Segunda-feira
        var monday = reconstructedProfile.WeeklySchedule[DayOfWeek.Monday];
        Assert.Equal(2, monday.Windows.Count);
        
        Assert.Equal(8, monday.Windows[0].Start.Hour);
        Assert.Equal(30, monday.Windows[0].Start.Minute);
        Assert.Equal(12, monday.Windows[0].End.Hour);
        Assert.Equal(0, monday.Windows[0].End.Minute);
        
        Assert.Equal(13, monday.Windows[1].Start.Hour);
        Assert.Equal(15, monday.Windows[1].Start.Minute);
        Assert.Equal(18, monday.Windows[1].End.Hour);
        Assert.Equal(45, monday.Windows[1].End.Minute);

        // Validar Sexta-feira
        var friday = reconstructedProfile.WeeklySchedule[DayOfWeek.Friday];
        Assert.Single(friday.Windows);
        Assert.Equal(9, friday.Windows[0].Start.Hour);
        Assert.Equal(14, friday.Windows[0].End.Hour);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _dbContainer.StopAsync();
    }
}
