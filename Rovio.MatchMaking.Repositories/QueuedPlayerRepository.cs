using Microsoft.EntityFrameworkCore;
using Rovio.MatchMaking;
using Rovio.MatchMaking.Repositories.Data;

public class QueuedPlayerRepository : IQueuedPlayerRepository
{
    private readonly AppDbContext _context;

    public QueuedPlayerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<QueuedPlayer>> GetAllQueuedPlayersAsync()
    {
        return await _context.QueuedPlayers.ToListAsync();
    }

    public async Task<QueuedPlayer> GetQueuedPlayerByIdAsync(Guid id)
    {
        return await _context.QueuedPlayers.FindAsync(id);
    }

    public async Task<QueuedPlayer> GetQueuedPlayerByPlayerIdAsync(Guid playerId)
    {
        return await _context.QueuedPlayers.FirstOrDefaultAsync(qp => qp.PlayerId == playerId);
    }

    public async Task<QueuedPlayer> CreateQueuedPlayerAsync(QueuedPlayer queuedPlayer)
    {
        queuedPlayer.Id = Guid.NewGuid(); // Assign a new unique ID
        _context.QueuedPlayers.Add(queuedPlayer);
        await _context.SaveChangesAsync();
        return queuedPlayer;
    }

    public async Task UpdateQueuedPlayerAsync(QueuedPlayer queuedPlayer)
    {
        _context.QueuedPlayers.Update(queuedPlayer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteQueuedPlayerAsync(Guid id)
    {
        var queuedPlayer = await _context.QueuedPlayers.FindAsync(id);
        if (queuedPlayer != null)
        {
            _context.QueuedPlayers.Remove(queuedPlayer);
            await _context.SaveChangesAsync();
        }
    }
}
