using System.Runtime.CompilerServices;

namespace SortTask.Application.Decorators;

public class ProgressRenderCommand<TResult>(
    ICommand<TResult> inner,
    IProgressRenderer progressRenderer
) : ICommand<TResult>
{
    public IEnumerable<CommandIteration<TResult>> Execute()
    {
        try
        {
            foreach (var iteration in inner.Execute())
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
