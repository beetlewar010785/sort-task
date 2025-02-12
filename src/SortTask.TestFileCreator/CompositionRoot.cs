using SortTask.Adapter;
using SortTask.Application;
using SortTask.Application.Decorators;
using SortTask.Domain.RowGeneration;

namespace SortTask.TestFileCreator;

public class CompositionRoot(
    ICommand<FeedRowCommand.Param, FeedRowCommand.Result> feedRowCommand,
    IList<IDisposable> disposables) : IDisposable
{
    public ICommand<FeedRowCommand.Param, FeedRowCommand.Result> FeedRowCommand => feedRowCommand;

    public static CompositionRoot Build(string filePath, long estimatedSize)
    {
        const int minRowNumber = 1;
        const int maxRowNumber = 100_000;
        const int minWordsInSentence = 2;
        const int maxWordsInSentence = 10;
        const int repeatRowPeriod = 1000;
        const int maxRepeatNumber = 2;
        const int refreshRepeatingRowsPeriod = 2;

        var file = File.Create(filePath);

        var rowWriter = new StreamRowStore(file, AdapterConst.Encoding);

        var rnd = new Random();
        var rowGenerator = new RowGenerationRepeater(
            inner: new RandomRowGenerator(
                rnd: rnd,
                minNumber: minRowNumber,
                maxNumber: maxRowNumber,
                minWordsInSentence: minWordsInSentence,
                maxWordsInSentence: maxWordsInSentence),
            rnd: rnd,
            repeatPeriod: repeatRowPeriod,
            maxRepeatNumber: maxRepeatNumber,
            refreshRepeatingRowsPeriod: refreshRepeatingRowsPeriod);

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

    public void Dispose()
    {
        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
    }
}