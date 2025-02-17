namespace SortTask.Application.Decorators;

public class StreamLengthProgressCalculatorCommand<TResult>(
    ICommand<TResult> inner,
    Stream stream
) : ICommand<TResult>
{
    public IEnumerable<CommandIteration<TResult>> Execute()
    {
        foreach (var iteration in inner.Execute())
        {
            var progress = (int)Math.Min(100 * stream.Position / stream.Length, 100);
            yield return iteration.SetProgress(progress);
        }
    }
}
