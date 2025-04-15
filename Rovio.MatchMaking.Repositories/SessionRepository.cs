using Microsoft.EntityFrameworkCore;
using Rovio.MatchMaking.Repositories.Data;
namespace Rovio.MatchMaking.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly AppDbContext _context;
    public SessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Session>> GetAllActiveSessionsAsync()
    {
        var activeSessions = _context.Sessions
        .Where(s => s.JoinedCount < 10) // Sessions with less than 10 players
        .Where(s => s.EndsAt > DateTime.UtcNow) // Sessions with EndsAt > now are active
        .OrderBy(s => s.CreatedAt); // Order by creation time;

        return await activeSessions.ToListAsync();
    }

    public async Task<Session> CreateNewAsync(int latencyLevel, int joinedCount, int gameTimeInMinutes)
    {
        // Validation checks for the parameters
        //TODO: move these checks to the logic layer??
        if (latencyLevel < 1 || latencyLevel > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(latencyLevel), "Latency level must be between 1 and 5.");
        }

        if (joinedCount < 0 || joinedCount > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(joinedCount), "Joined count must be between 0 and 10.");
        }

        if (gameTimeInMinutes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(gameTimeInMinutes), "Game time must be a positive number.");
        }

        var session = new Session
        {
            Id = Guid.NewGuid(),
            LatencyLevel = latencyLevel,
            JoinedCount = joinedCount,
            CreatedAt = DateTime.UtcNow,
            StartsAt = DateTime.UtcNow,
            EndsAt = DateTime.UtcNow.AddMinutes(gameTimeInMinutes)
        };

        await _context.Sessions.AddAsync(session);
        await _context.SaveChangesAsync();

        return session;
    }

    public async Task<Session> GetSessionById(Guid sessionId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        return session;
    }

    public async Task<IEnumerable<SessionPlayer>> GetSessionPlayersBySessionId(Guid sessionId)
    {
        var sessionPlayers = _context.SessionPlayers
        .Where(sp => sp.SessionId == sessionId);

        return await sessionPlayers.ToListAsync();
    }

    public async Task<SessionPlayer> AddPlayerToSessionAsync(Guid sessionId, Guid playerId)
    {
        // Check if the session exists
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session == null)
        {
            throw new ArgumentException("Session not found.");
        }

        // Check if the session has space for more players
        if (session.JoinedCount >= 10)
        {
            throw new InvalidOperationException("Session is full.");
        }

        // Check if the player is already in the session
        var existingSessionPlayer = await _context.SessionPlayers
            .FirstOrDefaultAsync(sp => sp.SessionId == sessionId && sp.PlayerId == playerId);
        if (existingSessionPlayer != null)
        {
            throw new InvalidOperationException("Player is already in the session.");
        }

        // Add the player to the session
        var sessionPlayer = new SessionPlayer
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            PlayerId = playerId,
            Status = "ATTENDED",
            Score = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        session.JoinedCount++; // Increase the player count in the session

        await _context.SessionPlayers.AddAsync(sessionPlayer);
        _context.Sessions.Update(session); // Update the session with the new JoinedCount
        await _context.SaveChangesAsync();

        return sessionPlayer;
    }

    public async Task<SessionPlayer> RemovePlayerFromSessionAsync(Guid sessionId, Guid playerId)
    {
        // Find the session player entry
        var sessionPlayer = await _context.SessionPlayers
            .FirstOrDefaultAsync(sp => sp.SessionId == sessionId && sp.PlayerId == playerId);
        if (sessionPlayer == null)
        {
            throw new ArgumentException("Player is not in the session.");
        }

        // Update the status to 'LEFT'
        sessionPlayer.Status = "LEFT";
        sessionPlayer.UpdatedAt = DateTime.UtcNow;

        // Update the session player entry
        _context.SessionPlayers.Update(sessionPlayer);

        // Find the session and decrease the JoinedCount if necessary
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session != null && session.JoinedCount > 0)
        {
            session.JoinedCount--;
            _context.Sessions.Update(session);
        }

        await _context.SaveChangesAsync();

        return sessionPlayer;
    }
}