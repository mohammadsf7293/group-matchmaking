namespace Rovio.MatchMaking;

public interface IQueuedPlayerRepository
{
    Task<IEnumerable<QueuedPlayer>> GetAllQueuedPlayersAsync();
    Task<QueuedPlayer> GetQueuedPlayerByIdAsync(Guid id);
    Task<QueuedPlayer> GetQueuedPlayerByPlayerIdAsync(Guid playerId);
    Task<QueuedPlayer> CreateQueuedPlayerAsync(QueuedPlayer queuedPlayer);
    Task UpdateQueuedPlayerAsync(QueuedPlayer queuedPlayer);
    Task DeleteQueuedPlayerAsync(Guid id);
}
