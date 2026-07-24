using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Rovio.MatchMaking.Application;
using Xunit;

namespace Rovio.MatchMaking.Console.Tests
{
    public class SessionMatchMakerTests
    {
        private readonly Mock<IQueuedPlayerRepository> _queuedPlayerRepositoryMock;
        private readonly Mock<ISessionRepository> _sessionRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly SessionMatchMaker _sessionMatchMaker;

        public SessionMatchMakerTests()
        {
            _queuedPlayerRepositoryMock = new Mock<IQueuedPlayerRepository>();
            _sessionRepositoryMock = new Mock<ISessionRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock
                .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(action => action());

            _sessionMatchMaker = new SessionMatchMaker(
                _queuedPlayerRepositoryMock.Object,
                _sessionRepositoryMock.Object,
                _unitOfWorkMock.Object,
                NullLogger<SessionMatchMaker>.Instance);
        }

        [Fact]
        public async Task CreatePlayersMapAsync_ShouldReturnCorrectMap1()
        {
            var players = new List<QueuedPlayer>
            {
                new QueuedPlayer { Id = Guid.NewGuid(), PlayerId = Guid.NewGuid(), LatencyLevel = 1, CreatedAt = DateTime.UtcNow },
                new QueuedPlayer { Id = Guid.NewGuid(), PlayerId = Guid.NewGuid(), LatencyLevel = 2, CreatedAt = DateTime.UtcNow }
            };
            _queuedPlayerRepositoryMock.Setup(qpr => qpr.GetAllQueuedPlayersAsync()).ReturnsAsync(players);

            var result = await _sessionMatchMaker.CreateQueuedPlayersMapAsync();

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

            var result = await _sessionMatchMaker.CreateQueuedPlayersMapAsync();

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
            var sessions = new List<Session>
            {
                new Session { Id = Guid.NewGuid(), LatencyLevel = 1, JoinedCount = 1, CreatedAt = DateTime.UtcNow },
                new Session { Id = Guid.NewGuid(), LatencyLevel = 2, JoinedCount = 2, CreatedAt = DateTime.UtcNow },
            };
            _sessionRepositoryMock.Setup(sr => sr.GetAllActiveSessionsAsync()).ReturnsAsync(sessions);

            var result = await _sessionMatchMaker.CreateActiveSessionsMapAsync();

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

            var result = await _sessionMatchMaker.CreateActiveSessionsMapAsync();

            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            Assert.Contains(1, result.Keys);
            Assert.Equal(1, result[1].Count);
            Assert.Equal(sessions[0].Id, result[1][0].Id);
            Assert.Contains(2, result.Keys);
            Assert.Equal(2, result[2].Count);
            Assert.Equal(sessions[1].Id, result[2][0].Id);
            Assert.Equal(sessions[2].Id, result[2][1].Id);
            Assert.Contains(3, result.Keys);
            Assert.Equal(3, result[3].Count);
            Assert.Equal(sessions[3].Id, result[3][0].Id);
            Assert.Equal(sessions[4].Id, result[3][1].Id);
            Assert.Equal(sessions[5].Id, result[3][2].Id);
            Assert.Contains(4, result.Keys);
            Assert.Equal(1, result[4].Count);
            Assert.Equal(sessions[6].Id, result[4][0].Id);
        }

        [Fact]
        public void RemoveAttendedPlayerIdsFromMap_ShouldRemoveAllAttendedPlayers()
        {
            var player1 = new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 1 };
            var player2 = new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 1 };
            var player3 = new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 2 };
            var queuedPlayersMap = new Dictionary<int, List<QueuedPlayer>>
            {
                { 1, new List<QueuedPlayer> { player1, player2 } },
                { 2, new List<QueuedPlayer> { player3 } }
            };
            var attendedPlayerIds = new List<Guid> { player1.PlayerId, player3.PlayerId };

            var result = _sessionMatchMaker.RemoveAttendedPlayerIdsFromMap(queuedPlayersMap, attendedPlayerIds);

            Assert.Equal(1, result.Count);
            Assert.Contains(1, result.Keys);
            Assert.Equal(player2.PlayerId, result[1][0].PlayerId);
        }

        [Fact]
        public void RemoveAttendedPlayerIdsFromMap_ShouldNotRemoveNonAttendedPlayers()
        {
            var player1 = new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 1 };
            var player2 = new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 1 };
            var player3 = new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 2 };
            var queuedPlayersMap = new Dictionary<int, List<QueuedPlayer>>
            {
                { 1, new List<QueuedPlayer> { player1, player2 } },
                { 2, new List<QueuedPlayer> { player3 } }
            };
            var attendedPlayerIds = new List<Guid>();

            var result = _sessionMatchMaker.RemoveAttendedPlayerIdsFromMap(queuedPlayersMap, attendedPlayerIds);

            Assert.Equal(2, result.Count);
            Assert.Contains(1, result.Keys);
            Assert.Contains(2, result.Keys);
            Assert.Equal(player1.PlayerId, result[1][0].PlayerId);
            Assert.Equal(player2.PlayerId, result[1][1].PlayerId);
            Assert.Equal(player3.PlayerId, result[2][0].PlayerId);
        }

        [Fact]
        public void RemoveAttendedPlayerIdsFromMap_ShouldHandleEmptyMap()
        {
            var queuedPlayersMap = new Dictionary<int, List<QueuedPlayer>>();
            var attendedPlayerIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var result = _sessionMatchMaker.RemoveAttendedPlayerIdsFromMap(queuedPlayersMap, attendedPlayerIds);

            Assert.Empty(result);
        }

        [Fact]
        public void RemoveAttendedPlayerIdsFromMap_ShouldRemoveAttendedPlayersAcrossDifferentLatencyLevels()
        {
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

            var result = _sessionMatchMaker.RemoveAttendedPlayerIdsFromMap(queuedPlayersMap, attendedPlayerIds);

            Assert.Equal(2, result.Count);
            Assert.Contains(1, result.Keys);
            Assert.Contains(2, result.Keys);
            Assert.Equal(1, result[1].Count);
            Assert.Equal(player2.PlayerId, result[1][0].PlayerId);
            Assert.Equal(player3.PlayerId, result[2][0].PlayerId);
        }

        [Fact]
        public async Task AddQueuedPlayersToActiveSessions_ShouldAddPlayersSuccessfully()
        {
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
                                  .ReturnsAsync(new SessionPlayer());
            _queuedPlayerRepositoryMock.Setup(repo => repo.DeleteQueuedPlayerAsync(It.IsAny<Guid>()))
                                       .Returns(Task.CompletedTask);

            var result = await _sessionMatchMaker.AddQueuedPlayersToActiveSessions(queuedPlayersMap, activeSessionsMap);

            Assert.Single(result);
            Assert.Equal(playerId, result[0]);
            Assert.Equal(1, activeSessionsMap[1][0].JoinedCount);
            _sessionRepositoryMock.Verify(repo => repo.AddPlayerToSessionAsync(sessionId, playerId), Times.Once);
        }

        [Fact]
        public async Task AddQueuedPlayersToActiveSessions_ShouldAssignPlayerToOnlyOneSession()
        {
            var playerId = Guid.NewGuid();
            var queuedPlayersMap = new Dictionary<int, List<QueuedPlayer>>
            {
                { 1, new List<QueuedPlayer> { new QueuedPlayer { Id = Guid.NewGuid(), PlayerId = playerId } } }
            };

            var session1 = new Session { Id = Guid.NewGuid(), JoinedCount = 0 };
            var session2 = new Session { Id = Guid.NewGuid(), JoinedCount = 0 };
            var activeSessionsMap = new Dictionary<int, List<Session>>
            {
                { 1, new List<Session> { session1, session2 } }
            };

            _sessionRepositoryMock.Setup(repo => repo.AddPlayerToSessionAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                                  .ReturnsAsync(new SessionPlayer());
            _queuedPlayerRepositoryMock.Setup(repo => repo.DeleteQueuedPlayerAsync(It.IsAny<Guid>()))
                                       .Returns(Task.CompletedTask);

            var result = await _sessionMatchMaker.AddQueuedPlayersToActiveSessions(queuedPlayersMap, activeSessionsMap);

            Assert.Single(result);
            Assert.Equal(1, session1.JoinedCount);
            Assert.Equal(0, session2.JoinedCount);
            _sessionRepositoryMock.Verify(repo => repo.AddPlayerToSessionAsync(session1.Id, playerId), Times.Once);
            _sessionRepositoryMock.Verify(repo => repo.AddPlayerToSessionAsync(session2.Id, playerId), Times.Never);
        }

        [Fact]
        public async Task AddQueuedPlayersToActiveSessions_ShouldAddPlayersSuccessfully2()
        {
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
                                  .ReturnsAsync(new SessionPlayer());
            _queuedPlayerRepositoryMock.Setup(repo => repo.DeleteQueuedPlayerAsync(It.IsAny<Guid>()))
                                       .Returns(Task.CompletedTask);

            var result = await _sessionMatchMaker.AddQueuedPlayersToActiveSessions(queuedPlayersMap, activeSessionsMap);

            Assert.Equal(4, result.Count);
            Assert.Equal(queuedPlayersMap[1][0].PlayerId, result[0]);
            Assert.Equal(queuedPlayersMap[2][0].PlayerId, result[1]);
            Assert.Equal(queuedPlayersMap[2][1].PlayerId, result[2]);
            Assert.Equal(queuedPlayersMap[3][0].PlayerId, result[3]);
            Assert.Equal(10, activeSessionsMap[1][0].JoinedCount);
            Assert.Equal(7, activeSessionsMap[2][0].JoinedCount);
            Assert.Equal(9, activeSessionsMap[3][0].JoinedCount);
        }

        [Fact]
        public async Task CreateSessionForRemainedPlayers_ShouldAddPlayersSuccessfully()
        {
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

            var queuedPlayersMapCp = new Dictionary<int, List<QueuedPlayer>>(queuedPlayersMap.Count, queuedPlayersMap.Comparer);
            foreach (var latencyLevel in queuedPlayersMap.Keys)
            {
                queuedPlayersMapCp[latencyLevel] = new List<QueuedPlayer>(queuedPlayersMap[latencyLevel]);
            }

            _sessionRepositoryMock.Setup(repo => repo.CreateNewAsync(1, 0, It.IsAny<int>()))
                                  .ReturnsAsync(new Session());
            _sessionRepositoryMock.Setup(repo => repo.CreateNewAsync(2, 0, It.IsAny<int>()))
                                  .ReturnsAsync(new Session());
            _sessionRepositoryMock.Setup(repo => repo.AddPlayerToSessionAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                                  .ReturnsAsync(new SessionPlayer());
            _queuedPlayerRepositoryMock.Setup(repo => repo.DeleteQueuedPlayerAsync(It.IsAny<Guid>()))
                                       .Returns(Task.CompletedTask);

            var result = await _sessionMatchMaker.CreateSessionForRemainedPlayers(queuedPlayersMap);

            Assert.Equal(5, result.Count);
            Assert.Equal(queuedPlayersMapCp[1][0].PlayerId, result[0]);
            Assert.Equal(queuedPlayersMapCp[1][1].PlayerId, result[1]);
            Assert.Equal(queuedPlayersMapCp[1][2].PlayerId, result[2]);
            Assert.Equal(queuedPlayersMapCp[2][0].PlayerId, result[3]);
            Assert.Equal(queuedPlayersMapCp[2][1].PlayerId, result[4]);
        }
    }
}
