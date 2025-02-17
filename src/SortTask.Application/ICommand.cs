namespace SortTask.Application;

public interface ICommand<TResult>
{
    IEnumerable<CommandIteration<TResult>> Execute();
}
