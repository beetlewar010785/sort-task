using System.Runtime.CompilerServices;

namespace SortTask.Application.Decorators;

public class ProgressRenderCommand<TResult>(
    ICommand<TResult> inner,
    IProgressRenderer progressRenderer
) : ICommand<TResult>
{
    public async IAsyncEnumerable<CommandIteration<TResult>> Execute(
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        try
        {
            await foreach (var iteration in inner.Execute(cancellationToken))
            {
                yield return iteration;
                progressRenderer.Render(iteration.ProgressPercent ?? 0, iteration.OperationName);
            }
        }
        finally
        {
            progressRenderer.Complete();
        }
    }
}
