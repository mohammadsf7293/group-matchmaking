using Microsoft.EntityFrameworkCore;
using Rovio.MatchMaking.Repositories.Data;

namespace Rovio.MatchMaking.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        // Non-relational providers (e.g. InMemory) do not support transactions.
        if (!_context.Database.IsRelational())
        {
            await action();
            return;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await action();
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
