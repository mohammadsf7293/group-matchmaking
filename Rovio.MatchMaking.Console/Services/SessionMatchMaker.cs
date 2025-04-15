using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.InMemory;
using Rovio.MatchMaking.Repositories;
using Rovio.MatchMaking.Repositories.Data;

namespace Rovio.MatchMaking.Console.Services;
public class SessionMatchMaker
{
    private readonly AppDbContext _context;
    private readonly ISessionRepository _sessionRepository;
    private readonly IQueuedPlayerRepository _queuedPlayerRepository;

    public SessionMatchMaker(AppDbContext context, IQueuedPlayerRepository queuedPlayerRepository, ISessionRepository sessionRepository)
    {
        _context = context;
        _queuedPlayerRepository = queuedPlayerRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task RunAsync()
    {
        try {
            // Step 1: Read Players from Queues and Create a HashMap
            var queuedPlayersMap = await CreateQueuedPlayersMapAsync();

            // Step 2: Read Active Sessions and Create a HashMap
            var activeSessionsMap = await CreateActiveSessionsMapAsync();

            var joinedSessionsPlayerIds = await AddQueuedPlayersToActiveSessions(queuedPlayersMap, activeSessionsMap);
            //TODO: send notification and matched event for joinedSessionsPlayerIds users

            queuedPlayersMap = await RemoveAttendedPlayerIdsFromMap(queuedPlayersMap, joinedSessionsPlayerIds);

            // Step 4: Create New Sessions if Necessary
            joinedSessionsPlayerIds = await CreateSessionForRemainedPlayers(queuedPlayersMap);
        } catch (Exception ex) {
            System.Console.WriteLine("Error while doing the matching operation!");
        }

        System.Console.WriteLine("Matchmaking process completed successfully!");
    }

    internal async Task<List<System.Guid>> AddQueuedPlayersToActiveSessions(Dictionary<int, List<QueuedPlayer>> queuedPlayersMap, Dictionary<int, List<Session>> activeSessionsMap)
    {
        // Preventing to use db transactions for test environment, to be able to mock db methods in this case
        var isTestEnvironment = _context.Database.IsInMemory();

        List<System.Guid> addedPlayerIds = new List<System.Guid>();

        foreach (var latencyLevel in queuedPlayersMap.Keys)
        {
            if (activeSessionsMap.ContainsKey(latencyLevel))
            {
                foreach (var queuedPlayer in queuedPlayersMap[latencyLevel])
                {
                    var sessions = activeSessionsMap[latencyLevel];
                    foreach (var session in sessions)
                    {
                        if (session.JoinedCount < 10)
                        {
                            IDbContextTransaction transaction = null;
                            if (!isTestEnvironment) {
                                transaction = await _context.Database.BeginTransactionAsync();    
                            }
                            // using var transaction = await _context.Database.BeginTransactionAsync();
                            try {
                                await _sessionRepository.AddPlayerToSessionAsync(session.Id, queuedPlayer.PlayerId);
                                session.JoinedCount++;
                                if (!isTestEnvironment) {
                                    _context.Sessions.Update(session);
                                }
                                await _queuedPlayerRepository.DeleteQueuedPlayerAsync(queuedPlayer.Id);
                                if (!isTestEnvironment) {
                                    await _context.SaveChangesAsync();
                                    await transaction.CommitAsync();   
                                }

                                addedPlayerIds.Add(queuedPlayer.PlayerId);
                            } catch (Exception ex)
                            {
                                // Rollback the transaction if any operation fails
                                if (!isTestEnvironment) {
                                    await transaction.RollbackAsync();
                                }
                                throw ex;
                            }
                        }
                    }
                }
            }
        }

        return addedPlayerIds;
    }

    internal async Task<List<System.Guid>> CreateSessionForRemainedPlayers(Dictionary<int, List<QueuedPlayer>> queuedPlayersMap)
    {
        // Preventing to use db transactions for test environment, to be able to mock db methods in this case
        var isTestEnvironment = _context.Database.IsInMemory();

        List<System.Guid> addedPlayerIds = new List<System.Guid>();
        
        foreach (var latencyLevel in queuedPlayersMap.Keys)
        {
            var remainingQueuedPlayers = queuedPlayersMap[latencyLevel];
            while (remainingQueuedPlayers.Count >= 2)
            {
                var sessionSize = remainingQueuedPlayers.Count >= 10 ? 10 : remainingQueuedPlayers.Count;
                
                IDbContextTransaction transaction = null;
                if (!isTestEnvironment) {
                    transaction = await _context.Database.BeginTransactionAsync();
                }
                try {
                    var newSession = await _sessionRepository.CreateNewAsync(latencyLevel, 0, 30);

                    for (int i = 0; i < sessionSize; i++)
                    {
                        var queuedPlayer = remainingQueuedPlayers[i];
                        // I have considered that the whole matching module is called by another service, so the existence of player and validity of playerId had been checked in other modules before 
                        await _sessionRepository.AddPlayerToSessionAsync(newSession.Id, queuedPlayer.PlayerId);
                        await _queuedPlayerRepository.DeleteQueuedPlayerAsync(queuedPlayer.Id);
                    }

                    if (!isTestEnvironment) {
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }

                    for (int i = 0; i < sessionSize; i++)
                    {
                        var queuedPlayer = remainingQueuedPlayers[i];
                        addedPlayerIds.Add(queuedPlayer.PlayerId);
                    }
                    remainingQueuedPlayers.RemoveRange(0, sessionSize);

                } catch (Exception ex) {
                    // Rollback the transaction if any operation fails
                    if (!isTestEnvironment) {
                        await transaction.RollbackAsync();
                    }
    
                    System.Console.WriteLine($"An error occurred during creating new session: PlayerIDs={string.Join(", ", remainingQueuedPlayers.GetRange(0, sessionSize))}, LatencyLevel={latencyLevel}, Error={ex.Message}");
                    throw ex;
                }
            }
        }

        return addedPlayerIds;
    }

    internal async Task<Dictionary<int, List<QueuedPlayer>>> RemoveAttendedPlayerIdsFromMap(Dictionary<int, List<QueuedPlayer>> queuedPlayersMap, List<System.Guid> attendedPlayerIds)
    {
        var attendedPlayerIdsMap = new Dictionary<System.Guid, bool>();
        foreach (var playerId in attendedPlayerIds) {
            attendedPlayerIdsMap[playerId] = true;
        }

        var newQueuedPlayersMap = new Dictionary<int, List<QueuedPlayer>>();
        foreach (var latencyLevel in queuedPlayersMap.Keys) {
            var newQueuedPlayersList = new List<QueuedPlayer>();
            foreach (var queuedPlayer in queuedPlayersMap[latencyLevel]) {
                if (!attendedPlayerIdsMap.ContainsKey(queuedPlayer.PlayerId)) {
                    newQueuedPlayersList.Add(queuedPlayer);
                }
            }
            if (newQueuedPlayersList.Count > 0) {
                newQueuedPlayersMap[latencyLevel] = newQueuedPlayersList;
            }
        }

        return newQueuedPlayersMap;
    }

    internal async Task<Dictionary<int, List<QueuedPlayer>>> CreateQueuedPlayersMapAsync()
    {
        //TODO: move this part to repoe
        // var playersQueue = await _context.QueuedPlayers
        //     .OrderBy(qp => qp.CreatedAt) // Order by queueing time
        //     .ToListAsync();
        var playersQueue = await _queuedPlayerRepository.GetAllQueuedPlayersAsync();

        var queudPlayersMap = new Dictionary<int, List<QueuedPlayer>>();

        foreach (var queuedPlayer in playersQueue)
        {
            if (!queudPlayersMap.ContainsKey(queuedPlayer.LatencyLevel))
            {
                queudPlayersMap[queuedPlayer.LatencyLevel] = new List<QueuedPlayer>();
            }
            queudPlayersMap[queuedPlayer.LatencyLevel].Add(queuedPlayer);
        }

        return queudPlayersMap;
    }

    internal async Task<Dictionary<int, List<Session>>> CreateActiveSessionsMapAsync()
    {
        //TODO: move this part to repoe
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
