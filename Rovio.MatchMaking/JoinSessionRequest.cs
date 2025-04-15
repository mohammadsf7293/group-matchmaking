namespace Rovio.MatchMaking;

public class JoinSessionRequest
{
    public Guid PlayerId { get; set; }
    public Guid SessionId { get; set; }
}