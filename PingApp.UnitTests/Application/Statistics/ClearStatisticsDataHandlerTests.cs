using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PingApp.Application.Features.Statistics;
using PingApp.Application.Interfaces;

namespace PingApp.UnitTests.Application.Statistics;

public class ClearStatisticsDataHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallClearAllStatuses_AndReturnUnit()
    {
        var repositoryMock = Substitute.For<IDeviceRepository>();
        var loggerMock = Substitute.For<ILogger<ClearStatisticsData.Handler>>();

        var handler = new ClearStatisticsData.Handler(
            repositoryMock,
            loggerMock);

        var result = await handler.Handle(new ClearStatisticsData.Command(), CancellationToken.None);

        result.Should().Be(Unit.Value);

        await repositoryMock.Received(1).ClearAllStatusesAsync(Arg.Any<CancellationToken>());
    }
}
