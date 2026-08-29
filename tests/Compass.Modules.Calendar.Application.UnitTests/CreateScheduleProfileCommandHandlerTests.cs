using Compass.Modules.Calendar.Application.Commands;
using Compass.Modules.Calendar.Contracts.DTOs;
using Compass.Modules.Calendar.Domain.Exceptions;
using Compass.Modules.Calendar.Domain.Model;
using Compass.Modules.Calendar.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace Compass.Modules.Calendar.Application.UnitTests;

public class CreateScheduleProfileCommandHandlerTests
{
    private readonly Mock<IScheduleProfileRepository> _repositoryMock;
    private readonly CreateScheduleProfileCommandHandler _handler;

    public CreateScheduleProfileCommandHandlerTests()
    {
        _repositoryMock = new Mock<IScheduleProfileRepository>();
        _handler = new CreateScheduleProfileCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldSaveProfileAndReturnDto()
    {
        // Arrange
        var command = new CreateScheduleProfileCommand(
            "America/Sao_Paulo",
            [
                new DayAvailabilityDto(
                    DayOfWeek.Monday,
                    [new TimeWindowDto(new TimeOnly(9, 0), new TimeOnly(18, 0))]
                )
            ]
        );

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.TimeZoneId.Should().Be("America/Sao_Paulo");
        result.WeeklyAvailability.Should().HaveCount(1);
        result.WeeklyAvailability[0].DayOfWeek.Should().Be(DayOfWeek.Monday);
        result.WeeklyAvailability[0].Windows.Should().HaveCount(1);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ScheduleProfile>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidTimeZone_ShouldThrowCalendarDomainException()
    {
        // Arrange
        var command = new CreateScheduleProfileCommand("Invalid_Zone", null);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<CalendarDomainException>();
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ScheduleProfile>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
