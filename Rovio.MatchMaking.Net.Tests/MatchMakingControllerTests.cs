using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Rovio.MatchMaking.Application;
using Xunit;

namespace Rovio.MatchMaking.Net.Tests
{
    public class MatchMakingControllerTests
    {
        private readonly Mock<IQueuedPlayerRepository> _queuedPlayerRepositoryMock;
        private readonly Mock<ISessionRepository> _sessionRepositoryMock;
        private readonly MatchMakingController _controller;

        public MatchMakingControllerTests()
        {
            _queuedPlayerRepositoryMock = new Mock<IQueuedPlayerRepository>();
            _sessionRepositoryMock = new Mock<ISessionRepository>();
            var service = new MatchMakingService(
                _queuedPlayerRepositoryMock.Object,
                _sessionRepositoryMock.Object,
                Mock.Of<ILogger<MatchMakingService>>());
            _controller = new MatchMakingController(service, Mock.Of<ILogger<MatchMakingController>>());
        }

        [Fact]
        public async Task QueuePlayerAsync_ShouldReturnBadRequest_WhenPlayerIsNull()
        {
            var result = await _controller.QueuePlayerAsync(null);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Player cannot be null", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task QueuePlayerAsync_ShouldReturnOk_WhenPlayerIsValid()
        {
            var player = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };
            _queuedPlayerRepositoryMock
                .Setup(qpr => qpr.CreateQueuedPlayerAsync(It.IsAny<QueuedPlayer>()))
                .ReturnsAsync((QueuedPlayer qp) => qp);
            _sessionRepositoryMock
                .Setup(sr => sr.IsPlayerInActiveSessionAsync(player.Id))
                .ReturnsAsync(false);

            var result = await _controller.QueuePlayerAsync(player);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Player queued successfully", okResult.Value.ToString());
        }

        [Fact]
        public async Task QueuePlayerAsync_ShouldReturnBadRequest_WhenPlayerHadBeenQueued()
        {
            var player = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };
            _queuedPlayerRepositoryMock
                .Setup(qpr => qpr.GetQueuedPlayerByPlayerIdAsync(player.Id))
                .ReturnsAsync(new QueuedPlayer());

            var result = await _controller.QueuePlayerAsync(player);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Player is already queued", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task QueuePlayerAsync_ShouldReturnBadRequest_WhenPlayerInActiveSession()
        {
            var player = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };
            _sessionRepositoryMock
                .Setup(sr => sr.IsPlayerInActiveSessionAsync(player.Id))
                .ReturnsAsync(true);

            var result = await _controller.QueuePlayerAsync(player);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Player is already in an active session", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task DequeuePlayerAsync_ShouldReturnBadRequest_WhenPlayerIsNull()
        {
            var result = await _controller.DequeuePlayerAsync(null);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Player cannot be null", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task DequeuePlayerAsync_ShouldReturnBadRequest_WhenPlayerHadNotBeenQueued()
        {
            var player = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };

            var result = await _controller.DequeuePlayerAsync(player);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Player is not queued", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task DequeuePlayerAsync_ShouldReturnOk_WhenPlayerHadBeenQueued()
        {
            var player = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };
            var queuedPlayer = QueuedPlayer.CreateQueuedPlayerFromPlayer(player);
            _queuedPlayerRepositoryMock
                .Setup(qpr => qpr.GetQueuedPlayerByPlayerIdAsync(player.Id))
                .ReturnsAsync(queuedPlayer);
            _queuedPlayerRepositoryMock
                .Setup(qpr => qpr.DeleteQueuedPlayerAsync(queuedPlayer.Id))
                .Returns(Task.CompletedTask);

            var result = await _controller.DequeuePlayerAsync(player);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Player dequeued successfully", okResult.Value.ToString());
        }

        [Fact]
        public async Task JoinSession_ShouldReturnBadRequest_WhenBodyIsNull()
        {
            var result = await _controller.GetSession(null);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Request body cannot be null", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task JoinSession_ShouldReturnOk_WhenPlayerHasASession()
        {
            var player1 = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };
            var player2 = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };

            var session = new Session { Id = Guid.NewGuid(), LatencyLevel = 1, JoinedCount = 2 };
            var sessionPlayers = new List<SessionPlayer>
            {
                new SessionPlayer { Id = Guid.NewGuid(), SessionId = session.Id, PlayerId = player1.Id },
                new SessionPlayer { Id = Guid.NewGuid(), SessionId = session.Id, PlayerId = player2.Id }
            };

            _sessionRepositoryMock.Setup(sr => sr.GetSessionById(session.Id)).ReturnsAsync(session);
            _sessionRepositoryMock.Setup(sr => sr.GetSessionPlayersBySessionId(session.Id)).ReturnsAsync(sessionPlayers);

            var result = await _controller.GetSession(new JoinSessionRequest { PlayerId = player1.Id, SessionId = session.Id });

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task JoinSession_ShouldReturnBadRequest_WhenPlayerHasNotAccess()
        {
            var player1 = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };
            var player2 = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };
            var player3 = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };

            var session = new Session { Id = Guid.NewGuid(), LatencyLevel = 1, JoinedCount = 2 };
            var sessionPlayers = new List<SessionPlayer>
            {
                new SessionPlayer { Id = Guid.NewGuid(), SessionId = session.Id, PlayerId = player1.Id },
                new SessionPlayer { Id = Guid.NewGuid(), SessionId = session.Id, PlayerId = player2.Id }
            };

            _sessionRepositoryMock.Setup(sr => sr.GetSessionById(session.Id)).ReturnsAsync(session);
            _sessionRepositoryMock.Setup(sr => sr.GetSessionPlayersBySessionId(session.Id)).ReturnsAsync(sessionPlayers);

            var result = await _controller.GetSession(new JoinSessionRequest { PlayerId = player3.Id, SessionId = session.Id });

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("You don't have permission to join this session", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task JoinSession_ShouldReturnBadRequest_WhenSessionIdIsInvalid()
        {
            var player1 = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };
            var invalidSessionId = Guid.NewGuid();

            var result = await _controller.GetSession(new JoinSessionRequest { PlayerId = player1.Id, SessionId = invalidSessionId });

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid Session", badRequestResult.Value.ToString());
        }
    }
}
