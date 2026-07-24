using FluentAssertions;
using Xunit;

namespace Rovio.MatchMaking.Tests;

public class QueuedPlayerLatencyTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(100, 1)]
    [InlineData(101, 2)]
    [InlineData(200, 2)]
    [InlineData(201, 3)]
    [InlineData(300, 3)]
    [InlineData(301, 4)]
    [InlineData(400, 4)]
    [InlineData(401, 5)]
    [InlineData(1000, 5)]
    public void ConvertLatencyToLevel_ShouldMapExpectedBuckets(int latencyMs, int expectedLevel)
    {
        QueuedPlayer.ConvertLatencyToLevel(latencyMs).Should().Be(expectedLevel);
    }

    [Fact]
    public void CreateQueuedPlayerFromPlayer_ShouldSetLatencyLevel()
    {
        var player = new Player { Id = Guid.NewGuid(), Name = "p", LatencyMilliseconds = 250 };
        var queued = QueuedPlayer.CreateQueuedPlayerFromPlayer(player);

        queued.PlayerId.Should().Be(player.Id);
        queued.LatencyLevel.Should().Be(3);
    }
}
