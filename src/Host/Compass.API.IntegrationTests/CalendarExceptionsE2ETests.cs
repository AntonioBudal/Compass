using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Compass.Modules.Calendar.Contracts.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Compass.API.IntegrationTests;

public class CalendarExceptionsE2ETests : IClassFixture<CompassApiFactory>
{
    private readonly CompassApiFactory _factory;

    public CalendarExceptionsE2ETests(CompassApiFactory factory)
    {
        _factory = factory;
    }

    private record CreateProfileRequest(string Timezone, Dictionary<string, List<WindowDto>> WeeklySchedule);
    private record WindowDto(string StartTime, string EndTime);
    private record CreateExceptionRequest(DateOnly Date, string StartTime, string EndTime, string Reason);

    [Fact]
    public async Task Exceptions_Should_Slice_Windows_And_Resolve_To_Utc_Correctly()
    {
        var client = _factory.CreateClient();
        var profileId = Guid.NewGuid();
        var testDate = new DateOnly(2026, 8, 17); // Monday

        // 1. CRIAR PERFIL E JANELA
        var req = new CreateProfileRequest("America/Sao_Paulo", new Dictionary<string, List<WindowDto>> {
            [DayOfWeek.Monday.ToString()] = new List<WindowDto> { new WindowDto("08:00", "12:00") }
        });
        var res = await client.PostAsJsonAsync($"/api/calendar/profiles/{profileId}", req);
        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);

        // 2. CRIAR EXCEÇÃO
        var excReq = new CreateExceptionRequest(testDate, "09:00", "10:00", "Médico");
        var res2 = await client.PostAsJsonAsync($"/api/calendar/profiles/{profileId}/exceptions", excReq);
        Assert.Equal(HttpStatusCode.NoContent, res2.StatusCode);

        // 3. CONSULTAR
        using var scope = _factory.Services.CreateScope();
        var availabilityQuery = scope.ServiceProvider.GetRequiredService<IAvailabilityQuery>();
        var availability = await availabilityQuery.GetAvailabilityAsync(profileId, testDate);

        // 4. ASSERT
        Assert.Equal(2, availability.Count);

        Assert.Equal(new DateTimeOffset(2026, 8, 17, 11, 0, 0, TimeSpan.Zero), availability[0].Start);
        Assert.Equal(new DateTimeOffset(2026, 8, 17, 12, 0, 0, TimeSpan.Zero), availability[0].End);
        Assert.Equal(TimeSpan.Zero, availability[0].Start.Offset);

        Assert.Equal(new DateTimeOffset(2026, 8, 17, 13, 0, 0, TimeSpan.Zero), availability[1].Start);
        Assert.Equal(new DateTimeOffset(2026, 8, 17, 15, 0, 0, TimeSpan.Zero), availability[1].End);
        Assert.Equal(TimeSpan.Zero, availability[1].Start.Offset);
    }
}
