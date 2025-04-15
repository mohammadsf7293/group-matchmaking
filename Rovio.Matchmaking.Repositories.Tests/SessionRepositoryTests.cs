using Microsoft.EntityFrameworkCore;
using Rovio.MatchMaking.Repositories;
using Rovio.MatchMaking.Repositories.Data;
using Xunit;

namespace Rovio.MatchMaking.Tests
{
    public class SessionRepositoryTests
    {
        private AppDbContext _context;
        private ISessionRepository _repository;

        public SessionRepositoryTests()
        {
            ProvideCleanDatabase();
        }
        
        // This method is called to provide a fresh DB context ensuring every test case is stateless
        protected void ProvideCleanDatabase()
        {
            // Setting up a clean in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique name
            .Options;

            _context = new AppDbContext(options);
            _repository = new SessionRepository(_context);
        }

        [Fact]
        public async Task GetAllActiveSessionsAsync_ShouldListActiveSessions()
        {
            ProvideCleanDatabase();

            // Arrange
            var sessionAtive1 = new Session { LatencyLevel = 1, JoinedCount = 1, CreatedAt = DateTime.UtcNow, EndsAt = DateTime.UtcNow.AddMinutes(30) };
            var sessionAtive2 = new Session { LatencyLevel = 1, JoinedCount = 1, CreatedAt = DateTime.UtcNow, EndsAt = DateTime.UtcNow.AddMinutes(30) };
            var sessionInactive = new Session { LatencyLevel = 1, JoinedCount = 1, CreatedAt = DateTime.UtcNow.AddMinutes(-50), EndsAt = DateTime.UtcNow.AddMinutes(-20) };

            // Act
            _context.Sessions.Add(sessionAtive1);
            _context.Sessions.Add(sessionAtive2);
            _context.Sessions.Add(sessionInactive);
            await _context.SaveChangesAsync();
            var fetchedSessions = await _repository.GetAllActiveSessionsAsync();

            // Assert
            Assert.NotNull(fetchedSessions);
            Assert.Equal(2, fetchedSessions.Count());
        }

        [Fact]
        public async Task GetSessionById_ShouldWorkOK()
        {
            ProvideCleanDatabase();

            // Arrange
            var session = new Session { LatencyLevel = 1, JoinedCount = 1, CreatedAt = DateTime.UtcNow, EndsAt = DateTime.UtcNow.AddMinutes(30) };
            
            // Act
            var insertedSession = _context.Sessions.Add(session);
            await _context.SaveChangesAsync();
            var fetchedSession = await _repository.GetSessionById(insertedSession.Entity.Id);

            // Assert
            Assert.NotNull(fetchedSession);
        }

        [Fact]
        public async Task GetSessionPlayersBySessionId_ShouldWorkOK()
        {
            ProvideCleanDatabase();

            // Arrange
            var session = new Session { LatencyLevel = 1, JoinedCount = 1, CreatedAt = DateTime.UtcNow, EndsAt = DateTime.UtcNow.AddMinutes(30) };
            
            // Act
            var insertedSession = _context.Sessions.Add(session);
            var sessionPlayer1 = new SessionPlayer{ SessionId = insertedSession.Entity.Id, PlayerId = new Guid() };
            var sessionPlayer2 = new SessionPlayer{ SessionId = insertedSession.Entity.Id, PlayerId = new Guid() };
            _context.SessionPlayers.Add(sessionPlayer1);
            _context.SessionPlayers.Add(sessionPlayer2);
            await _context.SaveChangesAsync();
            
            var fetchedSessionPlayers = await _repository.GetSessionPlayersBySessionId(session.Id);

            // Assert
            Assert.NotNull(fetchedSessionPlayers);
            Assert.Equal(2, fetchedSessionPlayers.Count());
        }

        [Fact]
        public async Task CreateNewAsync_ShouldCreateSession()
        {
            ProvideCleanDatabase();

            // Arrange
            var session = new Session { LatencyLevel = 1, JoinedCount = 1, CreatedAt = DateTime.UtcNow };

            // Act
            var insertedSession = await _repository.CreateNewAsync(session.LatencyLevel, session.JoinedCount, 10);
            await _context.SaveChangesAsync();
            var fetchedSession = await _context.Sessions.FindAsync(insertedSession.Id);

            // Assert
            Assert.NotNull(fetchedSession);
            Assert.Equal(session.JoinedCount, fetchedSession.JoinedCount);
            Assert.Equal(session.LatencyLevel, fetchedSession.LatencyLevel);
        }

        [Fact]
        public async Task AddPlayerToSessionAsync_ShouldAddPlayer()
        {
            ProvideCleanDatabase();

            // Arrange
            var playerId = Guid.NewGuid();
            var session = new Session { LatencyLevel = 1, JoinedCount = 0, CreatedAt = DateTime.UtcNow };

            // Act
            var insertedSession = await _repository.CreateNewAsync(session.LatencyLevel, session.JoinedCount, 10);
            await _context.SaveChangesAsync();
            var insertedSessionPlayer = await _repository.AddPlayerToSessionAsync(insertedSession.Id, playerId);
            await _context.SaveChangesAsync();
            var fetchedSessionPlayer = await _context.SessionPlayers.FindAsync(insertedSessionPlayer.Id);
            var fetchedSession = await _context.Sessions.FindAsync(insertedSession.Id);

            // Assert
            Assert.NotNull(fetchedSessionPlayer);
            Assert.NotNull(fetchedSession);
            Assert.Equal(playerId, fetchedSessionPlayer.PlayerId);
            Assert.Equal(insertedSession.Id, fetchedSessionPlayer.SessionId);
            Assert.Equal(1, fetchedSession.JoinedCount);
        }

        [Fact]
        public async Task RemovePlayerFromSessionAsync_ShouldAddPlayer()
        {
            ProvideCleanDatabase();

            // Arrange
            var playerId = Guid.NewGuid();
            var session = new Session { LatencyLevel = 1, JoinedCount = 1, CreatedAt = DateTime.UtcNow };
            var insertedSession = _context.Sessions.Add(session);
            
            var sessionPlayer = new SessionPlayer{ SessionId = insertedSession.Entity.Id, PlayerId = playerId };
            var insertedSessionPlayer = _context.SessionPlayers.Add(sessionPlayer);
            await _context.SaveChangesAsync();
            var fetchedSession = await _context.Sessions.FindAsync(insertedSession.Entity.Id);
            Assert.NotNull(fetchedSession);
            var fetchedSessionPlayer = await _context.SessionPlayers.FindAsync(insertedSessionPlayer.Entity.Id);
            Assert.NotNull(fetchedSessionPlayer);
            Assert.Equal(playerId, fetchedSessionPlayer.PlayerId);
            Assert.Equal("ATTENDED", fetchedSessionPlayer.Status);
            Assert.Equal(insertedSession.Entity.Id, fetchedSessionPlayer.SessionId);
            Assert.Equal(1, fetchedSession.JoinedCount);

            // Act
            await _repository.RemovePlayerFromSessionAsync(insertedSession.Entity.Id, insertedSessionPlayer.Entity.PlayerId);
            await _context.SaveChangesAsync();
            fetchedSessionPlayer = await _context.SessionPlayers.FindAsync(insertedSessionPlayer.Entity.Id);
            Assert.NotNull(fetchedSessionPlayer);
            Assert.Equal(playerId, fetchedSessionPlayer.PlayerId);
            Assert.Equal("LEFT", fetchedSessionPlayer.Status);
            Assert.Equal(insertedSession.Entity.Id, fetchedSessionPlayer.SessionId);
        }
    }
}
