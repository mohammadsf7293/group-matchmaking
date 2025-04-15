using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Rovio.MatchMaking.Console.Services;
using Rovio.MatchMaking.Repositories;
using Rovio.MatchMaking.Repositories.Data;
using Xunit;

namespace Rovio.MatchMaking.Console.Tests
{
    public class SessionMatchMakerTests
    {
        private readonly Mock<IQueuedPlayerRepository> _queuedPlayerRepositoryMock;
        private readonly Mock<ISessionRepository> _sessionRepositoryMock;
        private readonly AppDbContext _context;
        private readonly SessionMatchMaker _sessionMatchMaker;

        public SessionMatchMakerTests()
        {
            // Create an in-memory database for testing
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            //TODO: remove direct database related context and stuff
            _context = new AppDbContext(options);
            _queuedPlayerRepositoryMock = new Mock<IQueuedPlayerRepository>();
            _sessionRepositoryMock = new Mock<ISessionRepository>();

            // Initialize the SessionMatchmaker with mocked dependencies
            _sessionMatchMaker = new SessionMatchMaker(
                _context,
                _queuedPlayerRepositoryMock.Object,
                _sessionRepositoryMock.Object
            );
        }

        [Fact]
        public async Task CreatePlayersMapAsync_ShouldReturnCorrectMap1()
        {
            // Arrange
            var players = new List<QueuedPlayer>
            {
                new QueuedPlayer { Id = Guid.NewGuid(), PlayerId = Guid.NewGuid(), LatencyLevel = 1, CreatedAt = DateTime.UtcNow },
                new QueuedPlayer { Id = Guid.NewGuid(), PlayerId = Guid.NewGuid(), LatencyLevel = 2, CreatedAt = DateTime.UtcNow }
            };
            _queuedPlayerRepositoryMock.Setup(qpr => qpr.GetAllQueuedPlayersAsync()).ReturnsAsync(players);

            // Act
            var result = await _sessionMatchMaker.CreateQueuedPlayersMapAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(1, result.Keys);
            Assert.Equal(1, result[1].Count);
            Assert.Equal(players[0].PlayerId, result[1][0].PlayerId);
            Assert.Contains(2, result.Keys);
            Assert.Equal(1, result[2].Count);
            Assert.Equal(players[1].PlayerId, result[2][0].PlayerId);
        }

        [Fact]
        public async Task CreatePlayersMapAsync_ShouldReturnCorrectMap2()
        {
            // Arrange
            var players = new List<QueuedPlayer>
            {
                new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 1, CreatedAt = DateTime.UtcNow },
                new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 2, CreatedAt = DateTime.UtcNow },
                new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 2, CreatedAt = DateTime.UtcNow },
                new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 2, CreatedAt = DateTime.UtcNow },
                new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 3, CreatedAt = DateTime.UtcNow },
                new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 3, CreatedAt = DateTime.UtcNow },
                new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 4, CreatedAt = DateTime.UtcNow },
                new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 4, CreatedAt = DateTime.UtcNow },
                new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 4, CreatedAt = DateTime.UtcNow },
                new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 4, CreatedAt = DateTime.UtcNow },
                new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 4, CreatedAt = DateTime.UtcNow },
            };
            _queuedPlayerRepositoryMock.Setup(qpr => qpr.GetAllQueuedPlayersAsync()).ReturnsAsync(players);

            // Act
            var result = await _sessionMatchMaker.CreateQueuedPlayersMapAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            Assert.Contains(1, result.Keys);
            Assert.Equal(1, result[1].Count);
            Assert.Equal(players[0].PlayerId, result[1][0].PlayerId);
            Assert.Contains(2, result.Keys);
            Assert.Equal(3, result[2].Count);
            Assert.Equal(players[1].PlayerId, result[2][0].PlayerId);
            Assert.Equal(players[2].PlayerId, result[2][1].PlayerId);
            Assert.Equal(players[3].PlayerId, result[2][2].PlayerId);
        }


        [Fact]
        public async Task CreateActiveSessionsMapAsync_ShouldReturnCorrectMap1()
        {
            // Arrange
            var sessions = new List<Session>
            {
                new Session { Id = Guid.NewGuid(), LatencyLevel = 1, JoinedCount = 1, CreatedAt = DateTime.UtcNow },
                new Session { Id = Guid.NewGuid(), LatencyLevel = 2, JoinedCount = 2, CreatedAt = DateTime.UtcNow },
            };
            _sessionRepositoryMock.Setup(sr => sr.GetAllActiveSessionsAsync()).ReturnsAsync(sessions);

            // Act
            var result = await _sessionMatchMaker.CreateActiveSessionsMapAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(1, result.Keys);
            Assert.Equal(1, result[1].Count);
            Assert.Equal(sessions[0].Id, result[1][0].Id);
            Assert.Contains(2, result.Keys);
            Assert.Equal(1, result[2].Count);
            Assert.Equal(sessions[1].Id, result[2][0].Id);
        }

        [Fact]
        public async Task CreateActiveSessionsMapAsync_ShouldReturnCorrectMap2()
        {
            // Arrange
            var sessions = new List<Session>
            {
                new Session { Id = Guid.NewGuid(), LatencyLevel = 1, JoinedCount = 1, CreatedAt = DateTime.UtcNow },
                new Session { Id = Guid.NewGuid(), LatencyLevel = 2, JoinedCount = 2, CreatedAt = DateTime.UtcNow },
                new Session { Id = Guid.NewGuid(), LatencyLevel = 2, JoinedCount = 2, CreatedAt = DateTime.UtcNow },
                new Session { Id = Guid.NewGuid(), LatencyLevel = 3, JoinedCount = 1, CreatedAt = DateTime.UtcNow },
                new Session { Id = Guid.NewGuid(), LatencyLevel = 3, JoinedCount = 1, CreatedAt = DateTime.UtcNow },
                new Session { Id = Guid.NewGuid(), LatencyLevel = 3, JoinedCount = 1, CreatedAt = DateTime.UtcNow },
                new Session { Id = Guid.NewGuid(), LatencyLevel = 4, JoinedCount = 1, CreatedAt = DateTime.UtcNow },
            };
            _sessionRepositoryMock.Setup(sr => sr.GetAllActiveSessionsAsync()).ReturnsAsync(sessions);

            // Act
            var result = await _sessionMatchMaker.CreateActiveSessionsMapAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            Assert.Contains(1, result.Keys);
            Assert.Equal(1, result[1].Count);
            Assert.Equal(sessions[0].Id, result[1][0].Id);
            //
            Assert.Contains(2, result.Keys);
            Assert.Equal(2, result[2].Count);
            Assert.Equal(sessions[1].Id, result[2][0].Id);
            Assert.Equal(sessions[2].Id, result[2][1].Id);
            //
            Assert.Contains(3, result.Keys);
            Assert.Equal(3, result[3].Count);
            Assert.Equal(sessions[3].Id, result[3][0].Id);
            Assert.Equal(sessions[4].Id, result[3][1].Id);
            Assert.Equal(sessions[5].Id, result[3][2].Id);
            //
            Assert.Contains(4, result.Keys);
            Assert.Equal(1, result[4].Count);
            Assert.Equal(sessions[6].Id, result[4][0].Id);
        }

        [Fact]
        public async Task RemoveAttendedPlayerIdsFromMap_ShouldRemoveAllAttendedPlayers()
        {
            // Arrange
            var player1 = new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 1 };
            var player2 = new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 1 };
            var player3 = new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 2 };
            var queuedPlayersMap = new Dictionary<int, List<QueuedPlayer>>
            {
                { 1, new List<QueuedPlayer> { player1, player2 } },
                { 2, new List<QueuedPlayer> { player3 } }
            };
            var attendedPlayerIds = new List<Guid> { player1.PlayerId, player3.PlayerId };

            // Act
            var result = await _sessionMatchMaker.RemoveAttendedPlayerIdsFromMap(queuedPlayersMap, attendedPlayerIds);

            // Assert
            Assert.Equal(1, result.Count);
            Assert.Contains(1, result.Keys);
            Assert.Equal(player2.PlayerId, result[1][0].PlayerId);
        }

        [Fact]
        public async Task RemoveAttendedPlayerIdsFromMap_ShouldNotRemoveNonAttendedPlayers()
        {
            // Arrange
            var player1 = new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 1 };
            var player2 = new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 1 };
            var player3 = new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 2 };
            var queuedPlayersMap = new Dictionary<int, List<QueuedPlayer>>
            {
                { 1, new List<QueuedPlayer> { player1, player2 } },
                { 2, new List<QueuedPlayer> { player3 } }
            };
            var attendedPlayerIds = new List<Guid>(); // No attended players

            // Act
            var result = await _sessionMatchMaker.RemoveAttendedPlayerIdsFromMap(queuedPlayersMap, attendedPlayerIds);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(1, result.Keys);
            Assert.Contains(2, result.Keys);
            Assert.Equal(player1.PlayerId, result[1][0].PlayerId);
            Assert.Equal(player2.PlayerId, result[1][1].PlayerId);
            Assert.Equal(player3.PlayerId, result[2][0].PlayerId);
        }

        [Fact]
        public async Task RemoveAttendedPlayerIdsFromMap_ShouldHandleEmptyMap()
        {
            // Arrange
            var queuedPlayersMap = new Dictionary<int, List<QueuedPlayer>>();
            var attendedPlayerIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }; // Some attended player IDs

            // Act
            var result = await _sessionMatchMaker.RemoveAttendedPlayerIdsFromMap(queuedPlayersMap, attendedPlayerIds);

            // Assert
            Assert.Empty(result); // The result should also be an empty map
        }

        [Fact]
        public async Task RemoveAttendedPlayerIdsFromMap_ShouldRemoveAttendedPlayersAcrossDifferentLatencyLevels()
        {
            // Arrange
            var player1 = new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 1 };
            var player2 = new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 1 };
            var player3 = new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 2 };
            var player4 = new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 3 };
            var queuedPlayersMap = new Dictionary<int, List<QueuedPlayer>>
            {
                { 1, new List<QueuedPlayer> { player1, player2 } },
                { 2, new List<QueuedPlayer> { player3 } },
                { 3, new List<QueuedPlayer> { player4 } }
            };
            var attendedPlayerIds = new List<Guid> { player1.PlayerId, player4.PlayerId };

            // Act
            var result = await _sessionMatchMaker.RemoveAttendedPlayerIdsFromMap(queuedPlayersMap, attendedPlayerIds);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(1, result.Keys);
            Assert.Contains(2, result.Keys);
            Assert.Equal(1, result[1].Count);
            Assert.Equal(1, result[1].Count);
            Assert.Equal(player2.PlayerId, result[1][0].PlayerId);
            Assert.Equal(player3.PlayerId, result[2][0].PlayerId);
        }

        [Fact]
        public async Task AddQueuedPlayersToActiveSessions_ShouldAddPlayersSuccessfully()
        {
            // Arrange
            var playerId = Guid.NewGuid();
            var queuedPlayerId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            var queuedPlayersMap = new Dictionary<int, List<QueuedPlayer>>
            {
                { 1, new List<QueuedPlayer> { new QueuedPlayer { PlayerId = playerId, Id = queuedPlayerId } } }
            };
            
            var activeSessionsMap = new Dictionary<int, List<Session>>
            {
                { 1, new List<Session> { new Session { Id = sessionId, JoinedCount = 0 } } }
            };

            _sessionRepositoryMock.Setup(repo => repo.AddPlayerToSessionAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                                  .ReturnsAsync(new SessionPlayer{});
            _queuedPlayerRepositoryMock.Setup(repo => repo.DeleteQueuedPlayerAsync(It.IsAny<Guid>()))
                                       .Returns(Task.CompletedTask);

            // Act
            var result = await _sessionMatchMaker.AddQueuedPlayersToActiveSessions(queuedPlayersMap, activeSessionsMap);

            // Assert
            Assert.Single(result); // Only one player should be added
            Assert.Equal(playerId, result[0]);
            Assert.Equal(1, activeSessionsMap[1][0].JoinedCount); // Ensure the joined count increased
        }

        [Fact]
        public async Task AddQueuedPlayersToActiveSessions_ShouldAddPlayersSuccessfully2()
        {
            // Arrange
            var queuedPlayersMap = new Dictionary<int, List<QueuedPlayer>>
            {
                { 1, new List<QueuedPlayer> { 
                        new QueuedPlayer { Id = Guid.NewGuid(), PlayerId = Guid.NewGuid() },
                        new QueuedPlayer { Id = Guid.NewGuid(), PlayerId = Guid.NewGuid() },
                        new QueuedPlayer { Id = Guid.NewGuid(), PlayerId = Guid.NewGuid() }, 
                    } 
                },
                { 2, new List<QueuedPlayer> { 
                        new QueuedPlayer { Id = Guid.NewGuid(), PlayerId = Guid.NewGuid() },
                        new QueuedPlayer { Id = Guid.NewGuid(), PlayerId = Guid.NewGuid() },
                    } 
                },
                { 3, new List<QueuedPlayer> { 
                        new QueuedPlayer { Id = Guid.NewGuid(), PlayerId = Guid.NewGuid() }
                    } 
                }
            };
            
            var activeSessionsMap = new Dictionary<int, List<Session>>
            {
                { 1, new List<Session> { new Session { Id = Guid.NewGuid(), JoinedCount = 9 } } },
                { 2, new List<Session> { new Session { Id = Guid.NewGuid(), JoinedCount = 5 } } },
                { 3, new List<Session> { new Session { Id = Guid.NewGuid(), JoinedCount = 8 } } }
            };

            _sessionRepositoryMock.Setup(repo => repo.AddPlayerToSessionAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                                  .ReturnsAsync(new SessionPlayer{});
            _queuedPlayerRepositoryMock.Setup(repo => repo.DeleteQueuedPlayerAsync(It.IsAny<Guid>()))
                                       .Returns(Task.CompletedTask);

            // Act
            var result = await _sessionMatchMaker.AddQueuedPlayersToActiveSessions(queuedPlayersMap, activeSessionsMap);

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal(queuedPlayersMap[1][0].PlayerId, result[0]); // The first player of latency level of 1
            Assert.Equal(queuedPlayersMap[2][0].PlayerId, result[1]); // The first player of latency level of 2
            Assert.Equal(queuedPlayersMap[2][1].PlayerId, result[2]); // The second player of latency level of 2
            Assert.Equal(queuedPlayersMap[3][0].PlayerId, result[3]); // The first player of latency level of 3
        }

        [Fact]
        public async Task CreateSessionForRemainedPlayers_ShouldAddPlayersSuccessfully()
        {
            // Arrange
            var queuedPlayersMap = new Dictionary<int, List<QueuedPlayer>>
            {
                { 1, new List<QueuedPlayer> { 
                        new QueuedPlayer { Id = Guid.NewGuid(), PlayerId = Guid.NewGuid() },
                        new QueuedPlayer { Id = Guid.NewGuid(), PlayerId = Guid.NewGuid() },
                        new QueuedPlayer { Id = Guid.NewGuid(), PlayerId = Guid.NewGuid() }, 
                    } 
                },
                { 2, new List<QueuedPlayer> { 
                        new QueuedPlayer { Id = Guid.NewGuid(), PlayerId = Guid.NewGuid() },
                        new QueuedPlayer { Id = Guid.NewGuid(), PlayerId = Guid.NewGuid() },
                    } 
                },
                { 3, new List<QueuedPlayer> { 
                        new QueuedPlayer { Id = Guid.NewGuid(), PlayerId = Guid.NewGuid() }
                    } 
                }
            };

            // Cloning the queuedPlayersMap, because It will be manipulated by the code
                Dictionary<int, List<QueuedPlayer>> queuedPlayersMapCp = new Dictionary<int, List<QueuedPlayer>>(queuedPlayersMap.Count, queuedPlayersMap.Comparer);
                foreach (var latencyLevel in queuedPlayersMap.Keys)
                {
                    var list = new List<QueuedPlayer>();
                    foreach (var qp in queuedPlayersMap[latencyLevel]) {
                        list.Add(qp);
                    }
                    queuedPlayersMapCp[latencyLevel] = list;
                }
            //

            _sessionRepositoryMock.Setup(repo => repo.CreateNewAsync(1, 0, It.IsAny<int>()))
                                  .ReturnsAsync(new Session{});
            _sessionRepositoryMock.Setup(repo => repo.CreateNewAsync(2, 0, It.IsAny<int>()))
                                  .ReturnsAsync(new Session{});
            _sessionRepositoryMock.Setup(repo => repo.AddPlayerToSessionAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                                  .ReturnsAsync(new SessionPlayer{});
            _queuedPlayerRepositoryMock.Setup(repo => repo.DeleteQueuedPlayerAsync(It.IsAny<Guid>()))
                                       .Returns(Task.CompletedTask);

            // Act
            var result = await _sessionMatchMaker.CreateSessionForRemainedPlayers(queuedPlayersMap);

            // Assert
            Assert.Equal(5, result.Count);
            Assert.Equal(queuedPlayersMapCp[1][0].PlayerId, result[0]); // The first player of latency level of 1
            Assert.Equal(queuedPlayersMapCp[1][1].PlayerId, result[1]); // The second player of latency level of 1
            Assert.Equal(queuedPlayersMapCp[1][2].PlayerId, result[2]); // The third player of latency level of 1
            Assert.Equal(queuedPlayersMapCp[2][0].PlayerId, result[3]); // The first player of latency level of 2
            Assert.Equal(queuedPlayersMapCp[2][1].PlayerId, result[4]); // The second player of latency level of 2
        }
    }
}
