namespace Rovio.MatchMaking;

public class QueuedPlayer
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public int LatencyLevel { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static QueuedPlayer CreateQueuedPlayerFromPlayer(Player player)
    {
        return new QueuedPlayer
        {
            Id = Guid.NewGuid(),
            PlayerId = player.Id,
            LatencyLevel = ConvertLatencyToLevel(player.LatencyMilliseconds),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static int ConvertLatencyToLevel(int latencyMilliseconds)
    {
        if (latencyMilliseconds <= 100) return 1;
        if (latencyMilliseconds <= 200) return 2;
        if (latencyMilliseconds <= 300) return 3;
        if (latencyMilliseconds <= 400) return 4;
        return 5;
    }
}
