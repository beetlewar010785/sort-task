using SortTask.Adapter;
using SortTask.Adapter.StreamBTree;
using SortTask.Application;
using SortTask.Application.Decorators;
using SortTask.Domain;
using SortTask.Domain.BTree;

namespace SortTask.Sorter;

// I prefer not to use IoC containers due to the lack of compile-time checking
public class CompositionRoot(
    ICommand<BuildIndexCommand<StreamBTreeIndex>.Param, BuildIndexCommand<StreamBTreeIndex>.Result>
        buildIndexCommand,
    ICommand<SortRowsCommand<StreamBTreeIndex>.Param, SortRowsCommand<StreamBTreeIndex>.Result>
        sortRowsCommand,
    IList<IDisposable> disposables,
    IList<IInitializer> initializers) : IDisposable
{
    public ICommand<BuildIndexCommand<StreamBTreeIndex>.Param, BuildIndexCommand<StreamBTreeIndex>.Result>
        BuildIndexCommand => buildIndexCommand;

    public ICommand<SortRowsCommand<StreamBTreeIndex>.Param, SortRowsCommand<StreamBTreeIndex>.Result>
        SortRowsCommand => sortRowsCommand;

    public IEnumerable<IInitializer> Initializers => initializers;

    public static CompositionRoot Build(
        string unsortedFilePath,
        string indexFilePath,
        string sortedFilePath,
        BTreeOrder order
    )
    {
        var indexFactory = new StreamBTreeIndexFactory();
        var indexFile = File.Create(indexFilePath);
        var bTreeNodeReadWriter = new StreamBTreeNodeReadWriter(indexFile, order);
        var store = new StreamBTreeStore(bTreeNodeReadWriter);

        var unsortedFile = File.OpenRead(unsortedFilePath);
        var unsortedReadWriter = new StreamRowReadWriter(unsortedFile);
        var lookup = new StreamBTreeRowLookup(unsortedReadWriter);
        var indexer = new BTreeIndexer<StreamBTreeNode, StreamBTreeIndex, StreamBTreeNodeId>(
            store,
            new StreamBTreeNodeFactory(),
            new StreamBTreeIndexComparer(new RowComparer(), lookup),
            order);

        var indexTraverser = new BTreeIndexTraverser<StreamBTreeNode, StreamBTreeIndex, StreamBTreeNodeId>(
            store);

        var progressRenderer = new ConsoleProgressRenderer(AdapterConst.ProgressBarWidth);

        var unsortedFile2 = File.OpenRead(unsortedFilePath);
        var unsortedIterator = new StreamRowReadWriter(unsortedFile2);
        var buildIndexCommand = new BuildIndexCommand<StreamBTreeIndex>(indexFactory, unsortedIterator, indexer)
            .DecorateWithStreamLength(unsortedFile2)
            .DecorateWithProgressRender(progressRenderer);

        var sortedFile = File.Create(sortedFilePath);
        var outputRowReadWriter = new StreamRowReadWriter(sortedFile);
        var sortRowsCommand = new SortRowsCommand<StreamBTreeIndex>(indexTraverser, lookup, outputRowReadWriter)
            .DecorateWithPredefinedStreamLength(sortedFile, unsortedFile.Length)
            .DecorateWithProgressRender(progressRenderer);

        return new CompositionRoot(
            buildIndexCommand,
            sortRowsCommand,
            [unsortedFile, unsortedFile2, sortedFile, indexFile],
            [store]);
    }

    public void Dispose()
    {
        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
    }
}