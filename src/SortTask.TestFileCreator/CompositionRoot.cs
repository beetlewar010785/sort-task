using SortTask.Adapter;
using SortTask.Application;
using SortTask.Application.Decorators;
using SortTask.Domain.RowGeneration;

namespace SortTask.TestFileCreator;

public class CompositionRoot(
    ICommand<FeedRowCommand.Result> feedRowCommand,
    IList<IDisposable> disposables) : IDisposable
{
    public ICommand<FeedRowCommand.Result> FeedRowCommand => feedRowCommand;

    public void Dispose()
    {
        foreach (var disposable in disposables) disposable.Dispose();
        GC.SuppressFinalize(this);
    }

    public static CompositionRoot Build(string filePath, long estimatedSize)
    {
        var file = File.Create(filePath);
        file.SetLength(estimatedSize);

        var rowWriter = new StreamRowStore(file, AdapterConst.Encoding);

        var rnd = new Random();
        var rowGenerator = new RowGenerationRepeater(new RandomRowGenerator(rnd), rnd);

        var progressRenderer = new ConsoleProgressRenderer();

        var feedRowCommand = new FeedRowCommand(
                file,
                rowWriter,
                rowGenerator,
                estimatedSize)
            .DecorateWithPredefinedStreamLength(file, estimatedSize)
            .DecorateWithProgressRender(progressRenderer);

        return new CompositionRoot(feedRowCommand, [file]);
    }
}
