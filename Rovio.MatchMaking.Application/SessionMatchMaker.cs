using Microsoft.Extensions.Logging;

namespace Rovio.MatchMaking.Application;

public class SessionMatchMaker
{
    private const int MaxPlayersPerSession = 10;
    private const int MinPlayersPerSession = 2;
    private const int DefaultGameTimeMinutes = 30;

    private readonly ISessionRepository _sessionRepository;
    private readonly IQueuedPlayerRepository _queuedPlayerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SessionMatchMaker> _logger;

    public SessionMatchMaker(
        IQueuedPlayerRepository queuedPlayerRepository,
        ISessionRepository sessionRepository,
        IUnitOfWork unitOfWork,
        ILogger<SessionMatchMaker> logger)
    {
        _queuedPlayerRepository = queuedPlayerRepository;
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        var queuedPlayersMap = await CreateQueuedPlayersMapAsync();
        var activeSessionsMap = await CreateActiveSessionsMapAsync();

        _logger.LogInformation(
            "Matchmaking started: queuedLatencyBuckets={QueuedBuckets}, activeSessionBuckets={ActiveBuckets}",
            queuedPlayersMap.Count, activeSessionsMap.Count);

        var joinedSessionsPlayerIds = await AddQueuedPlayersToActiveSessions(queuedPlayersMap, activeSessionsMap);
        // Notifications / matched events for joinedSessionsPlayerIds would be published here.

        queuedPlayersMap = RemoveAttendedPlayerIdsFromMap(queuedPlayersMap, joinedSessionsPlayerIds);
        joinedSessionsPlayerIds = await CreateSessionForRemainedPlayers(queuedPlayersMap);

        _logger.LogInformation(
            "Matchmaking completed successfully. MatchedPlayers={MatchedCount}",
            joinedSessionsPlayerIds.Count);
    }

    internal async Task<List<Guid>> AddQueuedPlayersToActiveSessions(
        Dictionary<int, List<QueuedPlayer>> queuedPlayersMap,
        Dictionary<int, List<Session>> activeSessionsMap)
    {
        var addedPlayerIds = new List<Guid>();

        foreach (var latencyLevel in queuedPlayersMap.Keys)
        {
            if (!activeSessionsMap.TryGetValue(latencyLevel, out var sessions))
            {
                continue;
            }

            foreach (var queuedPlayer in queuedPlayersMap[latencyLevel])
            {
                foreach (var session in sessions)
                {
                    if (session.JoinedCount >= MaxPlayersPerSession)
                    {
                        continue;
                    }

                    try
                    {
                        await _unitOfWork.ExecuteInTransactionAsync(async () =>
                        {
                            await _sessionRepository.AddPlayerToSessionAsync(session.Id, queuedPlayer.PlayerId);
                            await _queuedPlayerRepository.DeleteQueuedPlayerAsync(queuedPlayer.Id);
                        });

                        // Track capacity in-memory only; repository already persisted JoinedCount.
                        session.JoinedCount++;
                        addedPlayerIds.Add(queuedPlayer.PlayerId);
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to add player {PlayerId} to session {SessionId}",
                            queuedPlayer.PlayerId, session.Id);
                        throw;
                    }
                }
            }
        }

        return addedPlayerIds;
    }

    internal async Task<List<Guid>> CreateSessionForRemainedPlayers(Dictionary<int, List<QueuedPlayer>> queuedPlayersMap)
    {
        var addedPlayerIds = new List<Guid>();

        foreach (var latencyLevel in queuedPlayersMap.Keys)
        {
            var remainingQueuedPlayers = queuedPlayersMap[latencyLevel];
            while (remainingQueuedPlayers.Count >= MinPlayersPerSession)
            {
                var sessionSize = Math.Min(MaxPlayersPerSession, remainingQueuedPlayers.Count);
                var playersForSession = remainingQueuedPlayers.Take(sessionSize).ToList();

                try
                {
                    await _unitOfWork.ExecuteInTransactionAsync(async () =>
                    {
                        var newSession = await _sessionRepository.CreateNewAsync(
                            latencyLevel, 0, DefaultGameTimeMinutes);

                        foreach (var queuedPlayer in playersForSession)
                        {
                            await _sessionRepository.AddPlayerToSessionAsync(newSession.Id, queuedPlayer.PlayerId);
                            await _queuedPlayerRepository.DeleteQueuedPlayerAsync(queuedPlayer.Id);
                        }
                    });

                    addedPlayerIds.AddRange(playersForSession.Select(p => p.PlayerId));
                    remainingQueuedPlayers.RemoveRange(0, sessionSize);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed creating session for LatencyLevel={LatencyLevel}, PlayerIds={PlayerIds}",
                        latencyLevel,
                        string.Join(", ", playersForSession.Select(p => p.PlayerId)));
                    throw;
                }
            }
        }

        return addedPlayerIds;
    }

    internal Dictionary<int, List<QueuedPlayer>> RemoveAttendedPlayerIdsFromMap(
        Dictionary<int, List<QueuedPlayer>> queuedPlayersMap,
        List<Guid> attendedPlayerIds)
    {
        var attendedPlayerIdsSet = attendedPlayerIds.ToHashSet();
        var newQueuedPlayersMap = new Dictionary<int, List<QueuedPlayer>>();

        foreach (var (latencyLevel, players) in queuedPlayersMap)
        {
            var remaining = players.Where(qp => !attendedPlayerIdsSet.Contains(qp.PlayerId)).ToList();
            if (remaining.Count > 0)
            {
                newQueuedPlayersMap[latencyLevel] = remaining;
            }
        }

        return newQueuedPlayersMap;
    }

    internal async Task<Dictionary<int, List<QueuedPlayer>>> CreateQueuedPlayersMapAsync()
    {
        var playersQueue = await _queuedPlayerRepository.GetAllQueuedPlayersAsync();
        var queuedPlayersMap = new Dictionary<int, List<QueuedPlayer>>();

        foreach (var queuedPlayer in playersQueue)
        {
            if (!queuedPlayersMap.ContainsKey(queuedPlayer.LatencyLevel))
            {
                queuedPlayersMap[queuedPlayer.LatencyLevel] = new List<QueuedPlayer>();
            }
            queuedPlayersMap[queuedPlayer.LatencyLevel].Add(queuedPlayer);
        }

        return queuedPlayersMap;
    }

    internal async Task<Dictionary<int, List<Session>>> CreateActiveSessionsMapAsync()
    {
        var activeSessions = await _sessionRepository.GetAllActiveSessionsAsync();
        var activeSessionsMap = new Dictionary<int, List<Session>>();

        foreach (var session in activeSessions)
        {
            if (!activeSessionsMap.ContainsKey(session.LatencyLevel))
            {
                activeSessionsMap[session.LatencyLevel] = new List<Session>();
            }
            activeSessionsMap[session.LatencyLevel].Add(session);
        }

        return activeSessionsMap;
    }
}
