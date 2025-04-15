using Rovio.MatchMaking;
public class QueuedPlayer
{
    public Guid Id { get; set; } // Unique identifier for each queued request
    public Guid PlayerId { get; set; } // ID of the player requesting to join a session
    public int LatencyLevel { get; set; } // Latency level of the player (1 to 5)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp when the record was created

    public static QueuedPlayer CreateQueuedPlayerFromPlayer(Player player)
    {
        return new QueuedPlayer
        {
            Id = Guid.NewGuid(), // Assign a new unique ID
            PlayerId = player.Id, // Reference the player's ID
            LatencyLevel = ConvertLatencyToLevel(player.LatencyMilliseconds), // Convert latency to a level
            CreatedAt = DateTime.UtcNow
        };
    }

    // Utility method to convert latency to a level (1 to 5)
    private static int ConvertLatencyToLevel(int latencyMilliseconds)
    {
        if (latencyMilliseconds <= 100) return 1; // Best latency
        if (latencyMilliseconds <= 200) return 2;
        if (latencyMilliseconds <= 300) return 3;
        if (latencyMilliseconds <= 400) return 4;
        return 5; // Worst latency
    }
}