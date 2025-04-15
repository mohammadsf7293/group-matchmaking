namespace Rovio.MatchMaking;

public class Player
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int LatencyMilliseconds { get; set; }
}