public class Session
{
    public Guid Id { get; set; } // Unique identifier for each session
    
    public int LatencyLevel { get; set; } // Latency level of the player (1 to 5)

    private int _joinedCount;
    public int JoinedCount 
    { 
        get => _joinedCount;
        set 
        {
            if (value < 0 || value > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(JoinedCount), "JoinedCount must be between 0 and 10.");
            }
            _joinedCount = value;
        }
    } // Number of players joined the session (max 10)

    public DateTime CreatedAt { get; set; }

    public DateTime StartsAt { get; set; } // Timestamp when the contest will start
    
    public DateTime EndsAt { get; set; } // Timestamp when the contest will end
}
