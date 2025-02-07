using SortTask.Domain;

namespace SortTask.Application.Decorators;

public static class CommandDecorationExtensions
{
    public static ICommand<TParam, TResult> DecorateWithProgressRender<TParam, TResult>(
        this ICommand<TParam, TResult> inner,
        IProgressRenderer progressRenderer
    )
    {
        return new ProgressRenderCommand<TParam, TResult>(inner, progressRenderer);
    }

    public static ICommand<TParam, TResult> DecorateWithPredefinedStreamLength<TParam, TResult>(
        this ICommand<TParam, TResult> inner,
        Stream stream,
        long estimatedSize
    )
    {
        return new PredefinedStreamLengthProgressCalculatorCommand<TParam, TResult>(
            inner,
            stream,
            estimatedSize);
    }

    public static ICommand<TParam, TResult> DecorateWithStreamLength<TParam, TResult>(
        this ICommand<TParam, TResult> inner,
        Stream stream
    )
    {
        return new StreamLengthProgressCalculatorCommand<TParam, TResult>(
            inner,
            stream);
    }
}