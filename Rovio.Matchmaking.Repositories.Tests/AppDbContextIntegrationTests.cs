using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Rovio.MatchMaking.Repositories.Data;
using Xunit;
using Xunit.Abstractions;

namespace Rovio.Matchmaking.Repositories.Tests
{
    public class AppDbContextIntegrationTests
    {
        [Fact]
        public void Test_AppDbContext_Should_Contain_QueuedPlayers_Table()
        {
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new AppDbContext(options);
            context.Database.EnsureCreated();

            // Check if 'QueuedPlayers' table exists
            Assert.True(TableExistsHelper(connection, "QueuedPlayers"), "'QueuedPlayers' table does not exist.");

            // Check if 'QueuedPlayers' table has primary key 'Id'
            Assert.True(CheckTableForPrimaryKeyHelper(connection, "QueuedPlayers"), "Table 'QueuedPlayers' does not have a primary key column named 'Id'.");
        }

        [Fact]
        public void Test_AppDbContext_Should_Contain_SessionPlayers_Table()
        {
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new AppDbContext(options);
            context.Database.EnsureCreated();

            // Check if 'SessionPlayers' table exists
            Assert.True(TableExistsHelper(connection, "SessionPlayers"), "'SessionPlayers' table does not exist.");

            // Check if 'SessionPlayers' table has primary key 'Id'
            Assert.True(CheckTableForPrimaryKeyHelper(connection, "SessionPlayers"), "Table 'SessionPlayers' does not have a primary key column named 'Id'.");
        }

        [Fact]
        public void Test_AppDbContext_Should_Contain_Sessions_Table()
        {
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new AppDbContext(options);
            context.Database.EnsureCreated();

            // Check if 'Sessions' table exists
            Assert.True(TableExistsHelper(connection, "Sessions"), "'Sessions' table does not exist.");

            // Check if 'Sessions' table has primary key 'Id'
            Assert.True(CheckTableForPrimaryKeyHelper(connection, "Sessions"), "Table 'Sessions' does not have a primary key column named 'Id'.");
        }

        // Helper function to check if a table exists in the SQLite database
        private bool TableExistsHelper(SqliteConnection connection, string tableName)
        {
            using var tableListCommand = connection.CreateCommand();
            tableListCommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";
            using var tableReader = tableListCommand.ExecuteReader();

            while (tableReader.Read())
            {
                var tableNameInDb = tableReader.GetString(0);
                if (tableNameInDb == tableName)
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckTableForPrimaryKeyHelper(SqliteConnection connection, string tableName)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info('{tableName}');";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var columnName = reader.GetString(1);
                var isPrimaryKey = reader.GetInt32(5);

                if (columnName == "Id" && isPrimaryKey == 1)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
