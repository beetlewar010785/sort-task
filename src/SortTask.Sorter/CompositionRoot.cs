using SortTask.Adapter;
using SortTask.Adapter.BTree;
using SortTask.Application;
using SortTask.Application.Decorators;
using SortTask.Domain;
using SortTask.Domain.BTree;

namespace SortTask.Sorter;

// I prefer not to use IoC containers due to the lack of compile-time checking
public class CompositionRoot<TOphValue>(
    ICommand<BuildIndexCommand.Param, BuildIndexCommand.Result>
        buildIndexCommand,
    ICommand<SortRowsCommand<TOphValue>.Param, SortRowsCommand<TOphValue>.Result>
        sortRowsCommand,
    OphCollisionDetector<TOphValue> ulongOphCollisionDetector,
    RowLookupCache rowLookupCache,
    BTreeStoreCache<TOphValue> bTreeStoreCache,
    IList<IDisposable> disposables,
    IList<IInitializer> initializers) : IDisposable
    where TOphValue : struct
{
    public ICommand<BuildIndexCommand.Param, BuildIndexCommand.Result>
        BuildIndexCommand => buildIndexCommand;

    public ICommand<SortRowsCommand<TOphValue>.Param, SortRowsCommand<TOphValue>.Result>
        SortRowsCommand => sortRowsCommand;

    public OphCollisionDetector<TOphValue> CollisionDetector => ulongOphCollisionDetector;

    public RowLookupCache RowLookupCache => rowLookupCache;

    public BTreeStoreCache<TOphValue> BTreeStoreCache => bTreeStoreCache;

    public IEnumerable<IInitializer> Initializers => initializers;

    public static CompositionRoot<ulong> BuildUlong(
        string unsortedFilePath,
        string indexFilePath,
        string sortedFilePath,
        BTreeOrder order
    )
    {
        return Build(
            unsortedFilePath,
            indexFilePath,
            sortedFilePath,
            order,
            new UlongOph(),
            new UlongOphReadWriter());
    }

    private static CompositionRoot<T> Build<T>(
        string unsortedFilePath,
        string indexFilePath,
        string sortedFilePath,
        BTreeOrder order,
        IOph<T> oph,
        IOphReadWriter<T> ophReadWriter)
        where T : struct
    {
        var encoding = AdapterConst.Encoding;

        var indexFile = File.Create(indexFilePath);
        var bTreeNodeReadWriter = new StreamBTreeNodeReadWriter<T>(indexFile, order, ophReadWriter);
        var streamBTreeStore = new StreamBTreeStore<T>(bTreeNodeReadWriter);
        var store = new BTreeStoreCache<T>(streamBTreeStore);

        var unsortedFile = File.OpenRead(unsortedFilePath);
        var rowLookup = new StreamRowStore(unsortedFile, encoding);
        var lookup = new RowLookupCache(rowLookup);

        var ophCollisionDetector = new OphCollisionDetector<T>(Comparer<T>.Default);
        var indexer = new Indexer<T>(
            store,
            new BTreeIndexComparer<T>(ophCollisionDetector, new RowComparer(), lookup),
            order,
            oph,
            encoding);

        var indexTraverser = new BTreeIndexTraverser<T>(store);

        var progressRenderer = new ConsoleProgressRenderer(AdapterConst.ProgressBarWidth);

        var unsortedFileIterationStream = File.OpenRead(unsortedFilePath);
        var unsortedIterator = new StreamRowStore(unsortedFileIterationStream, encoding);
        var buildIndexCommand = new BuildIndexCommand(unsortedIterator, indexer)
            .DecorateWithStreamLength(unsortedFileIterationStream)
            .DecorateWithProgressRender(progressRenderer);

        var sortedFile = File.Create(sortedFilePath);
        var outputRowReadWriter = new StreamRowStore(sortedFile, encoding);
        var sortRowsCommand = new SortRowsCommand<T>(indexTraverser, lookup, outputRowReadWriter)
            .DecorateWithPredefinedStreamLength(sortedFile, unsortedFile.Length)
            .DecorateWithProgressRender(progressRenderer);

        return new CompositionRoot<T>(
            buildIndexCommand,
            sortRowsCommand,
            ophCollisionDetector,
            lookup,
            store,
            [unsortedFile, unsortedFileIterationStream, sortedFile, indexFile],
            [streamBTreeStore]);
    }

    public void Dispose()
    {
        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
    }
}