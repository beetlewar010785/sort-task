using System.Runtime.CompilerServices;

namespace SortTask.Application.Decorators;

public class PredefinedStreamLengthProgressCalculatorCommand<TResult>(
    ICommand<TResult> inner,
    Stream stream,
    long estimatedSize
) : ICommand<TResult>
{
    public async IAsyncEnumerable<CommandIteration<TResult>> Execute(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var iteration in inner.Execute(cancellationToken))
        {
            var progress = (int)Math.Min(100 * stream.Position / estimatedSize, 100);
            yield return iteration.SetProgress(progress);
        }
    }
}
