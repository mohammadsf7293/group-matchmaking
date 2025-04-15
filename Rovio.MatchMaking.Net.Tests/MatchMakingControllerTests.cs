using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Rovio.MatchMaking.Net.Tests
{
    public class MatchMakingControllerTests
    {
        private readonly Mock<IQueuedPlayerRepository> _queuedPlayerRepositoryMock;
        private readonly Mock<ISessionRepository> _sessionRepositoryMock;
        private readonly MatchMakingController _controller;
        private readonly Mock<ILogger<MatchMakingController>> _loggerMock;

        public MatchMakingControllerTests()
        {
            _queuedPlayerRepositoryMock = new Mock<IQueuedPlayerRepository>();
            _sessionRepositoryMock = new Mock<ISessionRepository>();
            _loggerMock = new Mock<ILogger<MatchMakingController>>();
            _controller = new MatchMakingController(_queuedPlayerRepositoryMock.Object, _sessionRepositoryMock.Object);
        }

        [Fact]
        public async Task QueuePlayerAsync_ShouldReturnBadRequest_WhenPlayerIsNull()
        {
            // Act
            var result = await _controller.QueuePlayerAsync(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Player cannot be null", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task QueuePlayerAsync_ShouldReturnOk_WhenPlayerIsValid()
        {
            // Arrange
            var player = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };
            var _queuedPlayer = QueuedPlayer.CreateQueuedPlayerFromPlayer(player);
            _queuedPlayerRepositoryMock.Setup(qpr => qpr.CreateQueuedPlayerAsync(_queuedPlayer)).ReturnsAsync(new QueuedPlayer());

            // Act
            var result = await _controller.QueuePlayerAsync(player);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Player queued successfully", okResult.Value.ToString());
        }

        [Fact]
        public async Task QueuePlayerAsync_ShouldReturnBadRequest_WhenPlayerHadBeenQueued()
        {
            // Arrange
            var player = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };
            var _queuedPlayer = QueuedPlayer.CreateQueuedPlayerFromPlayer(player);
            _queuedPlayerRepositoryMock.Setup(qpr => qpr.GetQueuedPlayerByPlayerIdAsync(player.Id)).ReturnsAsync(new QueuedPlayer());
            _queuedPlayerRepositoryMock.Setup(qpr => qpr.CreateQueuedPlayerAsync(_queuedPlayer)).ReturnsAsync(new QueuedPlayer());

            // Act
            var result = await _controller.QueuePlayerAsync(player);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Player is already queued", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task DequeuePlayerAsync_ShouldReturnBadRequest_WhenPlayerIsNull()
        {
            // Act
            var result = await _controller.DequeuePlayerAsync(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Player cannot be null", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task DequeuePlayerAsync_ShouldReturnBadRequest_WhenPlayerHadNotBeenQueued()
        {
            // Arrange
            var player = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };

            // Act
            var result = await _controller.DequeuePlayerAsync(player);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Player is not queued", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task DequeuePlayerAsync_ShouldReturnOk_WhenPlayerHadBeenQueued()
        {
            // Arrange
            var player = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };
            var _queuedPlayer = QueuedPlayer.CreateQueuedPlayerFromPlayer(player);
            _queuedPlayerRepositoryMock.Setup(qpr => qpr.GetQueuedPlayerByPlayerIdAsync(player.Id)).ReturnsAsync(new QueuedPlayer());

            // Act
            var result = await _controller.QueuePlayerAsync(player);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Player is already queued", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task JoinSession_ShouldReturnBadRequest_WhenBodyIsNull()
        {
            // Arrange
            var player1 = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };
            var player2 = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };
            var player3 = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };

            var session = new Session {Id = Guid.NewGuid(), LatencyLevel = 1, JoinedCount = 2};
            var sessionPlayer1 = new SessionPlayer {Id = Guid.NewGuid(), SessionId = session.Id, PlayerId = player1.Id};
            var sessionPlayer2 = new SessionPlayer {Id = Guid.NewGuid(), SessionId = session.Id, PlayerId = player2.Id};

            var sessionPlayers = new List<SessionPlayer>();
            sessionPlayers.Add(sessionPlayer1);
            sessionPlayers.Add(sessionPlayer2);

            // var _queuedPlayer = QueuedPlayer.CreateQueuedPlayerFromPlayer(player);
            _sessionRepositoryMock.Setup(sr => sr.GetSessionById(session.Id)).ReturnsAsync(session);
            _sessionRepositoryMock.Setup(sr => sr.GetSessionPlayersBySessionId(session.Id)).ReturnsAsync(sessionPlayers);

            // Act
            var invalidSessionId = player1.Id;
            var result = await _controller.JoinSession(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Request body cannot be null", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task JoinSession_ShouldReturnOk_WhenPlayerHasASession()
        {
            // Arrange
            var player1 = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };
            var player2 = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };

            var session = new Session {Id = Guid.NewGuid(), LatencyLevel = 1, JoinedCount = 2};
            var sessionPlayer1 = new SessionPlayer {Id = Guid.NewGuid(), SessionId = session.Id, PlayerId = player1.Id};
            var sessionPlayer2 = new SessionPlayer {Id = Guid.NewGuid(), SessionId = session.Id, PlayerId = player2.Id};

            var sessionPlayers = new List<SessionPlayer>();
            sessionPlayers.Add(sessionPlayer1);
            sessionPlayers.Add(sessionPlayer2);

            // var _queuedPlayer = QueuedPlayer.CreateQueuedPlayerFromPlayer(player);
            _sessionRepositoryMock.Setup(sr => sr.GetSessionById(session.Id)).ReturnsAsync(session);
            _sessionRepositoryMock.Setup(sr => sr.GetSessionPlayersBySessionId(session.Id)).ReturnsAsync(sessionPlayers);

            // Act
            var result = await _controller.JoinSession(new JoinSessionRequest{PlayerId= player1.Id, SessionId= session.Id});

            // Assert
            var oKResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(oKResult.Value);
        }

        [Fact]
        public async Task JoinSession_ShouldReturnBadRequest_WhenPlayerHasNotAccess()
        {
            // Arrange
            var player1 = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };
            var player2 = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };
            var player3 = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };

            var session = new Session {Id = Guid.NewGuid(), LatencyLevel = 1, JoinedCount = 2};
            var sessionPlayer1 = new SessionPlayer {Id = Guid.NewGuid(), SessionId = session.Id, PlayerId = player1.Id};
            var sessionPlayer2 = new SessionPlayer {Id = Guid.NewGuid(), SessionId = session.Id, PlayerId = player2.Id};

            var sessionPlayers = new List<SessionPlayer>();
            sessionPlayers.Add(sessionPlayer1);
            sessionPlayers.Add(sessionPlayer2);

            // var _queuedPlayer = QueuedPlayer.CreateQueuedPlayerFromPlayer(player);
            _sessionRepositoryMock.Setup(sr => sr.GetSessionById(session.Id)).ReturnsAsync(session);
            _sessionRepositoryMock.Setup(sr => sr.GetSessionPlayersBySessionId(session.Id)).ReturnsAsync(sessionPlayers);

            // Act
            var result = await _controller.JoinSession(new JoinSessionRequest{PlayerId= player3.Id, SessionId= session.Id});

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("You don't have permission to join this session", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task JoinSession_ShouldReturnBadRequest_WhenSessionIdIsInvalid()
        {
            // Arrange
            var player1 = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };
            var player2 = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };
            var player3 = new Player { Id = Guid.NewGuid(), Name = "TestPlayer", LatencyMilliseconds = 50 };

            var session = new Session {Id = Guid.NewGuid(), LatencyLevel = 1, JoinedCount = 2};
            var sessionPlayer1 = new SessionPlayer {Id = Guid.NewGuid(), SessionId = session.Id, PlayerId = player1.Id};
            var sessionPlayer2 = new SessionPlayer {Id = Guid.NewGuid(), SessionId = session.Id, PlayerId = player2.Id};

            var sessionPlayers = new List<SessionPlayer>();
            sessionPlayers.Add(sessionPlayer1);
            sessionPlayers.Add(sessionPlayer2);

            // var _queuedPlayer = QueuedPlayer.CreateQueuedPlayerFromPlayer(player);
            _sessionRepositoryMock.Setup(sr => sr.GetSessionById(session.Id)).ReturnsAsync(session);
            _sessionRepositoryMock.Setup(sr => sr.GetSessionPlayersBySessionId(session.Id)).ReturnsAsync(sessionPlayers);

            // Act
            var invalidSessionId = player1.Id;
            var result = await _controller.JoinSession(new JoinSessionRequest{PlayerId= player1.Id, SessionId= invalidSessionId});

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Invalid Session", badRequestResult.Value.ToString());
        }
    }
}
