namespace SortTask.Application;

public interface ICommand<in TParam, TResult>
{
    IAsyncEnumerable<CommandIteration<TResult>> Execute(TParam param, CancellationToken cancellationToken);
}