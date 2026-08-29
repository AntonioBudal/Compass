using Compass.Modules.Calendar.Application.Queries;
using Compass.Modules.Calendar.Domain.Model;
using Compass.Modules.Calendar.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace Compass.Modules.Calendar.Application.UnitTests;

public class GetScheduleProfileByIdQueryHandlerTests
{
    private readonly Mock<IScheduleProfileRepository> _repositoryMock;
    private readonly GetScheduleProfileByIdQueryHandler _handler;

    public GetScheduleProfileByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IScheduleProfileRepository>();
        _handler = new GetScheduleProfileByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenProfileExists_ShouldReturnDto()
    {
        // Arrange
        var profileId = Guid.CreateVersion7();
        var profile = ScheduleProfile.Create(new TimeZoneId("America/Sao_Paulo"), null);
        _repositoryMock.Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var query = new GetScheduleProfileByIdQuery(profileId);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.TimeZoneId.Should().Be("America/Sao_Paulo");
    }

    [Fact]
    public async Task HandleAsync_WhenProfileDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var profileId = Guid.CreateVersion7();
        _repositoryMock.Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ScheduleProfile?)null);

        var query = new GetScheduleProfileByIdQuery(profileId);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeNull();
    }
}
