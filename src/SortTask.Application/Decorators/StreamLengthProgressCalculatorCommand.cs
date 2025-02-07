using System.Runtime.CompilerServices;

namespace SortTask.Application.Decorators;

public class StreamLengthProgressCalculatorCommand<TParam, TResult>(
    ICommand<TParam, TResult> inner,
    Stream stream
) : ICommand<TParam, TResult>
{
    public async IAsyncEnumerable<CommandIteration<TResult>> Execute(
        TParam param,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var iteration in inner.Execute(param, cancellationToken))
        {
            var progress = (int)Math.Min(100 * stream.Position / stream.Length, 100);
            yield return iteration.SetProgress(progress);
        }
    }
}