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
        return await _context.Sessions
            .Where(s => s.JoinedCount < 10)
            .Where(s => s.EndsAt > DateTime.UtcNow)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<Session> CreateNewAsync(int latencyLevel, int joinedCount, int gameTimeInMinutes)
    {
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
        return await _context.Sessions.FindAsync(sessionId);
    }

    public async Task<IEnumerable<SessionPlayer>> GetSessionPlayersBySessionId(Guid sessionId)
    {
        return await _context.SessionPlayers
            .Where(sp => sp.SessionId == sessionId)
            .ToListAsync();
    }

    public async Task<bool> IsPlayerInActiveSessionAsync(Guid playerId)
    {
        var now = DateTime.UtcNow;
        return await _context.SessionPlayers
            .Where(sp => sp.PlayerId == playerId && sp.Status != SessionPlayerStatus.Left)
            .Join(
                _context.Sessions.Where(s => s.EndsAt > now),
                sp => sp.SessionId,
                s => s.Id,
                (sp, s) => sp)
            .AnyAsync();
    }

    public async Task<SessionPlayer> AddPlayerToSessionAsync(Guid sessionId, Guid playerId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session == null)
        {
            throw new ArgumentException("Session not found.");
        }

        if (session.JoinedCount >= 10)
        {
            throw new InvalidOperationException("Session is full.");
        }

        var existingSessionPlayer = await _context.SessionPlayers
            .FirstOrDefaultAsync(sp => sp.SessionId == sessionId && sp.PlayerId == playerId);
        if (existingSessionPlayer != null)
        {
            throw new InvalidOperationException("Player is already in the session.");
        }

        var sessionPlayer = new SessionPlayer
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            PlayerId = playerId,
            Status = SessionPlayerStatus.Attended,
            Score = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        session.JoinedCount++;

        await _context.SessionPlayers.AddAsync(sessionPlayer);
        _context.Sessions.Update(session);
        await _context.SaveChangesAsync();

        return sessionPlayer;
    }

    public async Task<SessionPlayer> RemovePlayerFromSessionAsync(Guid sessionId, Guid playerId)
    {
        var sessionPlayer = await _context.SessionPlayers
            .FirstOrDefaultAsync(sp => sp.SessionId == sessionId && sp.PlayerId == playerId);
        if (sessionPlayer == null)
        {
            throw new ArgumentException("Player is not in the session.");
        }

        sessionPlayer.Status = SessionPlayerStatus.Left;
        sessionPlayer.UpdatedAt = DateTime.UtcNow;
        _context.SessionPlayers.Update(sessionPlayer);

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
