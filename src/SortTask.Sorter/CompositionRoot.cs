using SortTask.Adapter;
using SortTask.Adapter.MemoryBTree;
using SortTask.Application;
using SortTask.Application.Decorators;
using SortTask.Domain;
using SortTask.Domain.BTree;

namespace SortTask.Sorter;

// I prefer not to use IoC containers due to the lack of compile-time checking
public class CompositionRoot(
    ICommand<BuildIndexCommand<MemoryBTreeIndex>.Param, BuildIndexCommand<MemoryBTreeIndex>.Result>
        buildIndexCommand,
    ICommand<SortRowsCommand<MemoryBTreeIndex>.Param, SortRowsCommand<MemoryBTreeIndex>.Result>
        sortRowsCommand,
    IList<IDisposable> disposables) : IDisposable
{
    public ICommand<BuildIndexCommand<MemoryBTreeIndex>.Param, BuildIndexCommand<MemoryBTreeIndex>.Result>
        BuildIndexCommand => buildIndexCommand;

    public ICommand<SortRowsCommand<MemoryBTreeIndex>.Param, SortRowsCommand<MemoryBTreeIndex>.Result>
        SortRowsCommand => sortRowsCommand;

    public static CompositionRoot Build(
        string inputFilePath,
        string outputFilePath,
        BTreeOrder order
    )
    {
        var inputFile = File.OpenRead(inputFilePath);
        var outputFile = File.Create(outputFilePath);

        var indexFactory = new MemoryBTreeIndexFactory();
        var store = new MemoryBTreeStore();
        var lookup = new MemoryBTreeRowLookup();
        var indexer = new BTreeIndexer<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>(
            store,
            new MemoryBTreeNodeFactory(),
            new MemoryBTreeIndexComparer(new RowComparer()),
            order);

        var rowReader = new StreamRowReader(inputFile, AdapterConst.Encoding);
        var rowWriter = new StreamRowWriter(outputFile, AdapterConst.Encoding);
        var indexTraverser = new BTreeIndexTraverser<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>(
            store);

        var progressRenderer = new ConsoleProgressRenderer(AdapterConst.ProgressBarWidth);

        var buildIndexCommand = new BuildIndexCommand<MemoryBTreeIndex>(
                indexFactory,
                rowReader,
                indexer)
            .DecorateWithStreamLength(inputFile)
            .DecorateWithProgressRender(progressRenderer);

        var sortRowsCommand = new SortRowsCommand<MemoryBTreeIndex>(
                indexTraverser,
                lookup,
                rowWriter)
            .DecorateWithPredefinedStreamLength(outputFile, inputFile.Length)
            .DecorateWithProgressRender(progressRenderer);

        return new CompositionRoot(buildIndexCommand, sortRowsCommand, [inputFile, outputFile]);
    }

    public void Dispose()
    {
        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
    }
}