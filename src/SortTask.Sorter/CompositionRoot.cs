using SortTask.Adapter;
using SortTask.Adapter.BTree;
using SortTask.Application;
using SortTask.Application.Decorators;
using SortTask.Domain;
using SortTask.Domain.BTree;

namespace SortTask.Sorter;

// I prefer not to use IoC containers due to the lack of compile-time checking
public class CompositionRoot<TOphValue>(
    ICommand<BuildIndexCommand.Result>
        buildIndexCommand,
    ICommand<SortRowsCommand<TOphValue>.Result>
        sortRowsCommand,
    OphCollisionDetector<TOphValue> ophCollisionDetector,
    IList<IDisposable> disposables) : IDisposable
    where TOphValue : struct
{
    public ICommand<BuildIndexCommand.Result>
        BuildIndexCommand => buildIndexCommand;

    public ICommand<SortRowsCommand<TOphValue>.Result>
        SortRowsCommand => sortRowsCommand;

    public OphCollisionDetector<TOphValue> CollisionDetector => ophCollisionDetector;

    public void Dispose()
    {
        foreach (var disposable in disposables) disposable.Dispose();

        GC.SuppressFinalize(this);
    }
}

public static class CompositionRootBuilder
{
    public static CompositionRoot<OphValue> Build(
        string unsortedFilePath,
        string indexFilePath,
        string sortedFilePath,
        BTreeOrder order,
        Oph oph
    )
    {
        return Build(
            unsortedFilePath,
            indexFilePath,
            sortedFilePath,
            order,
            oph,
            new OphReadWriter(oph.NumWords),
            new OphComparer());
    }

    private static CompositionRoot<T> Build<T>(
        string unsortedFilePath,
        string indexFilePath,
        string sortedFilePath,
        BTreeOrder order,
        IOph<T> oph,
        IOphReadWriter<T> ophReadWriter,
        IComparer<T> ophComparer)
        where T : struct
    {
        var encoding = AdapterConst.Encoding;
        var unsortedFile = File.OpenRead(unsortedFilePath);
        var sortedFile = File.Create(sortedFilePath);
        sortedFile.SetLength(unsortedFile.Length);

        var indexFile = File.Create(indexFilePath);
        var store = new StreamBTreeStore<T>(indexFile, order, ophReadWriter);

        var lookup = new StreamRowStore(unsortedFile, encoding);

        var ophCollisionDetector = new OphCollisionDetector<T>(ophComparer);
        var indexer = new BTreeIndexer<T>(
            store,
            new BTreeIndexComparer<T>(ophCollisionDetector, new RowComparer(), lookup),
            order,
            oph,
            encoding);

        var indexTraverser = new BTreeIndexTraverser<T>(store);

        var progressRenderer = new ConsoleProgressRenderer();

        var unsortedFileIterationStream = File.OpenRead(unsortedFilePath);
        var unsortedIterator = new StreamRowStore(unsortedFileIterationStream, encoding);
        var buildIndexCommand = new BuildIndexCommand(unsortedIterator, indexer)
            .DecorateWithStreamLength(unsortedFileIterationStream)
            .DecorateWithProgressRender(progressRenderer);

        var outputRowReadWriter = new StreamRowStore(sortedFile, encoding);
        var sortRowsCommand = new SortRowsCommand<T>(indexTraverser, lookup, outputRowReadWriter)
            .DecorateWithPredefinedStreamLength(sortedFile, unsortedFile.Length)
            .DecorateWithProgressRender(progressRenderer);

        return new CompositionRoot<T>(
            buildIndexCommand,
            sortRowsCommand,
            ophCollisionDetector,
            [store, unsortedFile, unsortedFileIterationStream, sortedFile, indexFile]);
    }
}
