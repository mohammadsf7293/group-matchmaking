using Microsoft.Extensions.Logging;

namespace Rovio.MatchMaking.Application;

public class MatchMakingService
{
    private readonly IQueuedPlayerRepository _queuedPlayerRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ILogger<MatchMakingService> _logger;

    public MatchMakingService(
        IQueuedPlayerRepository queuedPlayerRepository,
        ISessionRepository sessionRepository,
        ILogger<MatchMakingService> logger)
    {
        _queuedPlayerRepository = queuedPlayerRepository;
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    public async Task<OperationResult> QueuePlayerAsync(Player player)
    {
        if (player == null)
        {
            return OperationResult.Fail("Player cannot be null");
        }

        var existingQueuedPlayer = await _queuedPlayerRepository.GetQueuedPlayerByPlayerIdAsync(player.Id);
        if (existingQueuedPlayer != null)
        {
            return OperationResult.Fail("Player is already queued");
        }

        if (await _sessionRepository.IsPlayerInActiveSessionAsync(player.Id))
        {
            return OperationResult.Fail("Player is already in an active session");
        }

        var queuedPlayer = QueuedPlayer.CreateQueuedPlayerFromPlayer(player);
        await _queuedPlayerRepository.CreateQueuedPlayerAsync(queuedPlayer);

        _logger.LogInformation(
            "Player queued: PlayerId={PlayerId}, LatencyMs={LatencyMs}, LatencyLevel={LatencyLevel}",
            player.Id, player.LatencyMilliseconds, queuedPlayer.LatencyLevel);

        return OperationResult.Ok();
    }

    public async Task<OperationResult> DequeuePlayerAsync(Player player)
    {
        if (player == null)
        {
            return OperationResult.Fail("Player cannot be null");
        }

        var queuedPlayer = await _queuedPlayerRepository.GetQueuedPlayerByPlayerIdAsync(player.Id);
        if (queuedPlayer == null)
        {
            return OperationResult.Fail("Player is not queued");
        }

        await _queuedPlayerRepository.DeleteQueuedPlayerAsync(queuedPlayer.Id);
        _logger.LogInformation("Player dequeued: PlayerId={PlayerId}", player.Id);

        return OperationResult.Ok();
    }

    public async Task<OperationResult<object>> GetSessionForPlayerAsync(JoinSessionRequest request)
    {
        if (request == null)
        {
            return OperationResult<object>.Fail("Request body cannot be null");
        }

        var session = await _sessionRepository.GetSessionById(request.SessionId);
        if (session == null)
        {
            return OperationResult<object>.Fail("Invalid Session");
        }

        var sessionPlayers = (await _sessionRepository.GetSessionPlayersBySessionId(request.SessionId)).ToList();
        if (!sessionPlayers.Any(sp => sp.PlayerId == request.PlayerId))
        {
            return OperationResult<object>.Fail("You don't have permission to join this session");
        }

        return OperationResult<object>.Ok(new { session, sessionPlayers });
    }
}
