namespace Rovio.MatchMaking;

public interface IUnitOfWork
{
    Task ExecuteInTransactionAsync(Func<Task> action);
}
