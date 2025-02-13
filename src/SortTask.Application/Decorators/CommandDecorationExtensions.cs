namespace SortTask.Application.Decorators;

public static class CommandDecorationExtensions
{
    public static ICommand<TResult> DecorateWithProgressRender<TResult>(
        this ICommand<TResult> inner,
        IProgressRenderer progressRenderer
    )
    {
        return new ProgressRenderCommand<TResult>(inner, progressRenderer);
    }

    public static ICommand<TResult> DecorateWithPredefinedStreamLength<TResult>(
        this ICommand<TResult> inner,
        Stream stream,
        long estimatedSize
    )
    {
        return new PredefinedStreamLengthProgressCalculatorCommand<TResult>(
            inner,
            stream,
            estimatedSize);
    }

    public static ICommand<TResult> DecorateWithStreamLength<TResult>(
        this ICommand<TResult> inner,
        Stream stream
    )
    {
        return new StreamLengthProgressCalculatorCommand<TResult>(inner, stream);
    }
}
