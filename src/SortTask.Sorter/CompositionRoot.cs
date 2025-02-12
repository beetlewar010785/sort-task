using SortTask.Adapter;
using SortTask.Adapter.BTree;
using SortTask.Application;
using SortTask.Application.Decorators;
using SortTask.Domain;
using SortTask.Domain.BTree;
using Oph = SortTask.Adapter.Oph;

namespace SortTask.Sorter;

// I prefer not to use IoC containers due to the lack of compile-time checking
public class CompositionRoot(
    ICommand<BuildIndexCommand.Param, BuildIndexCommand.Result>
        buildIndexCommand,
    ICommand<SortRowsCommand.Param, SortRowsCommand.Result>
        sortRowsCommand,
    OphCollisionDetector ophCollisionDetector,
    RowLookupCache rowLookupCache,
    BTreeStoreCache bTreeStoreCache,
    IList<IDisposable> disposables,
    IList<IInitializer> initializers) : IDisposable
{
    public ICommand<BuildIndexCommand.Param, BuildIndexCommand.Result>
        BuildIndexCommand => buildIndexCommand;

    public ICommand<SortRowsCommand.Param, SortRowsCommand.Result>
        SortRowsCommand => sortRowsCommand;

    public OphCollisionDetector CollisionDetector => ophCollisionDetector;

    public RowLookupCache RowLookupCache => rowLookupCache;

    public BTreeStoreCache BTreeStoreCache => bTreeStoreCache;

    public IEnumerable<IInitializer> Initializers => initializers;

    public static CompositionRoot Build(
        string unsortedFilePath,
        string indexFilePath,
        string sortedFilePath,
        BTreeOrder order
    )
    {
        var encoding = AdapterConst.Encoding;

        var oph = new Oph();
        var indexFile = File.Create(indexFilePath);
        var bTreeNodeReadWriter = new StreamBTreeNodeReadWriter(indexFile, order);
        var streamBTreeStore = new StreamBTreeStore(bTreeNodeReadWriter);
        var store = new BTreeStoreCache(streamBTreeStore);

        var unsortedFile = File.OpenRead(unsortedFilePath);
        var rowLookup = new StreamRowStore(unsortedFile, encoding, oph);
        var lookup = new RowLookupCache(rowLookup);

        var ophCollisionDetector = new OphCollisionDetector(new OphComparer());
        var indexer = new Indexer(
            store,
            new BTreeIndexComparer(ophCollisionDetector, new RowComparer(), lookup),
            order);

        var indexTraverser = new BTreeIndexTraverser(
            store);

        var progressRenderer = new ConsoleProgressRenderer(AdapterConst.ProgressBarWidth);

        var unsortedFileIterationStream = File.OpenRead(unsortedFilePath);
        var unsortedIterator = new StreamRowStore(unsortedFileIterationStream, encoding, oph);
        var buildIndexCommand = new BuildIndexCommand(unsortedIterator, indexer)
            .DecorateWithStreamLength(unsortedFileIterationStream)
            .DecorateWithProgressRender(progressRenderer);

        var sortedFile = File.Create(sortedFilePath);
        var outputRowReadWriter = new StreamRowStore(sortedFile, encoding, oph);
        var sortRowsCommand = new SortRowsCommand(indexTraverser, lookup, outputRowReadWriter)
            .DecorateWithPredefinedStreamLength(sortedFile, unsortedFile.Length)
            .DecorateWithProgressRender(progressRenderer);

        return new CompositionRoot(
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