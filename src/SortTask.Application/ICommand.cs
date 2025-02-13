namespace SortTask.Application;

public interface ICommand<TResult>
{
    IAsyncEnumerable<CommandIteration<TResult>> Execute(CancellationToken cancellationToken);
}
