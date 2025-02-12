using System.Runtime.CompilerServices;
using SortTask.Domain;

namespace SortTask.Application.Decorators;

public class ProgressRenderCommand<TParam, TResult>(
    ICommand<TParam, TResult> inner,
    IProgressRenderer progressRenderer
) : ICommand<TParam, TResult>
{
    public async IAsyncEnumerable<CommandIteration<TResult>> Execute(
        TParam param,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        try
        {
            await foreach (var iteration in inner.Execute(param, cancellationToken))
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