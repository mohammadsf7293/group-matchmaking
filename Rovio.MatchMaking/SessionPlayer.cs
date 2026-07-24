namespace Rovio.MatchMaking;

public class SessionPlayer
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid PlayerId { get; set; }
    public string Status { get; set; } = SessionPlayerStatus.Attended;
    public int Score { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
