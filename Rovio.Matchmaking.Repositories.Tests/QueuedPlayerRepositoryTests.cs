using Microsoft.EntityFrameworkCore;
using Rovio.MatchMaking.Repositories;
using Rovio.MatchMaking.Repositories.Data;
using Xunit;

namespace Rovio.MatchMaking.Tests
{
    public class QueuedPlayerRepositoryTests
    {
        private AppDbContext _context;
        private IQueuedPlayerRepository _repository;

        public QueuedPlayerRepositoryTests()
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
            _repository = new QueuedPlayerRepository(_context);
        }

        [Fact]
        public async Task AddQueuedPlayerAsync_ShouldAddPlayer()
        {
            ProvideCleanDatabase();

            // Arrange
            var playerId = Guid.NewGuid();
            var queuedPlayer = new QueuedPlayer { PlayerId = playerId, LatencyLevel = 1, CreatedAt = DateTime.UtcNow };

            // Act
            var insertedQueuedPlayer = await _repository.CreateQueuedPlayerAsync(queuedPlayer);
            await _context.SaveChangesAsync();
            var result = await _context.QueuedPlayers.FindAsync(insertedQueuedPlayer.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(playerId, result.PlayerId);
        }

        [Fact]
        public async Task DeleteQueuedPlayerAsync_ShouldRemovePlayer()
        {
            ProvideCleanDatabase();

            // Arrange
            var playerId = Guid.NewGuid();
            var queuedPlayer = new QueuedPlayer { PlayerId = playerId, LatencyLevel = 1, CreatedAt = DateTime.UtcNow };
            var inserted = await _repository.CreateQueuedPlayerAsync(queuedPlayer);

            // Act
            await _repository.DeleteQueuedPlayerAsync(inserted.Id);
            var result = await _context.QueuedPlayers.FindAsync(inserted.Id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllQueuedPlayersAsync_ShouldReturnFifoOrder()
        {
            ProvideCleanDatabase();

            var older = new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 1, CreatedAt = DateTime.UtcNow.AddMinutes(-5) };
            var newer = new QueuedPlayer { PlayerId = Guid.NewGuid(), LatencyLevel = 1, CreatedAt = DateTime.UtcNow };
            await _context.QueuedPlayers.AddRangeAsync(newer, older);
            await _context.SaveChangesAsync();

            var result = (await _repository.GetAllQueuedPlayersAsync()).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(older.PlayerId, result[0].PlayerId);
            Assert.Equal(newer.PlayerId, result[1].PlayerId);
        }

        [Fact]
        public async Task UpdateQueuedPlayerAsync_ShouldUpdatePlayer()
        {
            ProvideCleanDatabase();

            // Arrange
            var playerId = Guid.NewGuid();
            var queuedPlayer = new QueuedPlayer { PlayerId = playerId, LatencyLevel = 1, CreatedAt = DateTime.UtcNow };

            // Act
            var insertedQueuedPlayer = await _repository.CreateQueuedPlayerAsync(queuedPlayer);
            await _context.SaveChangesAsync();
            insertedQueuedPlayer.LatencyLevel = 2;
            _context.QueuedPlayers.Update(insertedQueuedPlayer);
            await _context.SaveChangesAsync();
            var result = await _context.QueuedPlayers.FindAsync(insertedQueuedPlayer.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.LatencyLevel);
        }

        [Fact]
        public async Task GetQueuedPlayerByIdAsync_ShouldGetPlayer()
        {
            ProvideCleanDatabase();

            // Arrange
            var playerId = Guid.NewGuid();
            var queuedPlayer = new QueuedPlayer { PlayerId = playerId, LatencyLevel = 1, CreatedAt = DateTime.UtcNow };

            // Act
            var insertedQueuedPlayer = await _repository.CreateQueuedPlayerAsync(queuedPlayer);
            await _context.SaveChangesAsync();

            var result = await _repository.GetQueuedPlayerByIdAsync(insertedQueuedPlayer.Id);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetQueuedPlayerByPlayerIdAsync_ShouldGetPlayer()
        {
            ProvideCleanDatabase();

            // Arrange
            var playerId = Guid.NewGuid();
            var queuedPlayer = new QueuedPlayer { PlayerId = playerId, LatencyLevel = 1, CreatedAt = DateTime.UtcNow };

            // Act
            var insertedQueuedPlayer = await _repository.CreateQueuedPlayerAsync(queuedPlayer);
            await _context.SaveChangesAsync();

            var result = await _repository.GetQueuedPlayerByPlayerIdAsync(insertedQueuedPlayer.PlayerId);

            // Assert
            Assert.NotNull(result);
        }
    }
}
