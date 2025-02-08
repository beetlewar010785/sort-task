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
    IList<IDisposable> disposables) : IDisposable
{
    public ICommand<BuildIndexCommand<StreamBTreeIndex>.Param, BuildIndexCommand<StreamBTreeIndex>.Result>
        BuildIndexCommand => buildIndexCommand;

    public ICommand<SortRowsCommand<StreamBTreeIndex>.Param, SortRowsCommand<StreamBTreeIndex>.Result>
        SortRowsCommand => sortRowsCommand;

    // public static CompositionRoot Build(
    //     string inputFilePath,
    //     string outputFilePath,
    //     BTreeOrder order
    // )
    // {
    //     var inputFile = File.OpenRead(inputFilePath);
    //     var outputFile = File.Create(outputFilePath);
    //
    //     var indexFactory = new MemoryBTreeIndexFactory();
    //     var store = new MemoryBTreeStore();
    //     var lookup = new MemoryBTreeRowLookup();
    //     var indexer = new BTreeIndexer<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>(
    //         store,
    //         new MemoryBTreeNodeFactory(),
    //         new MemoryBTreeIndexComparer(new RowComparer()),
    //         order);
    //
    //     var streamReader = new StreamReader(inputFile, AdapterConst.Encoding, leaveOpen: true);
    //     var rowReader = new StreamRowReader(streamReader);
    //
    //     var streamWriter = new StreamWriter(outputFile, AdapterConst.Encoding, leaveOpen: true);
    //     var rowWriter = new StreamRowWriter(streamWriter);
    //
    //     var indexTraverser = new BTreeIndexTraverser<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>(
    //         store);
    //
    //     var progressRenderer = new ConsoleProgressRenderer(AdapterConst.ProgressBarWidth);
    //
    //     var buildIndexCommand = new BuildIndexCommand<MemoryBTreeIndex>(
    //             indexFactory,
    //             rowReader,
    //             indexer)
    //         .DecorateWithStreamLength(inputFile)
    //         .DecorateWithProgressRender(progressRenderer);
    //
    //     var sortRowsCommand = new SortRowsCommand<MemoryBTreeIndex>(
    //             indexTraverser,
    //             lookup,
    //             rowWriter)
    //         .DecorateWithPredefinedStreamLength(outputFile, inputFile.Length)
    //         .DecorateWithProgressRender(progressRenderer);
    //
    //     return new CompositionRoot(
    //         buildIndexCommand,
    //         sortRowsCommand,
    //         [streamReader, streamWriter, inputFile, outputFile]);
    // }

    public static CompositionRoot Build(
        string inputFilePath,
        string indexFilePath,
        string outputFilePath,
        BTreeOrder order
    )
    {
        var inputFile = File.OpenRead(inputFilePath);
        var indexFile = File.Create(indexFilePath);
        var outputFile = File.Create(outputFilePath);

        var indexFactory = new StreamBTreeIndexFactory();
        var bTreeNodeReadWriter = new StreamBTreeNodeReadWriter(indexFile, order);
        var store = new StreamBTreeStore(bTreeNodeReadWriter);
        var lookupStreamReader = new StreamReader(inputFile, AdapterConst.Encoding, leaveOpen: true);
        var lookup = new StreamBTreeRowLookup(lookupStreamReader);
        var indexer = new BTreeIndexer<StreamBTreeNode, StreamBTreeIndex, StreamBTreeNodeId>(
            store,
            new StreamBTreeNodeFactory(),
            new StreamBTreeIndexComparer(new RowComparer(), lookup),
            order);

        //var streamReader = new StreamReader(inputFile, AdapterConst.Encoding, leaveOpen: true);
        var rowReader = new StreamRowReader(lookupStreamReader);

        var streamWriter = new StreamWriter(outputFile, AdapterConst.Encoding, leaveOpen: true);
        var rowWriter = new StreamRowWriter(streamWriter);

        var indexTraverser = new BTreeIndexTraverser<StreamBTreeNode, StreamBTreeIndex, StreamBTreeNodeId>(
            store);

        var progressRenderer = new ConsoleProgressRenderer(AdapterConst.ProgressBarWidth);

        var buildIndexCommand = new BuildIndexCommand<StreamBTreeIndex>(
                indexFactory,
                rowReader,
                indexer)
            .DecorateWithStreamLength(inputFile)
            .DecorateWithProgressRender(progressRenderer);

        var sortRowsCommand = new SortRowsCommand<StreamBTreeIndex>(
                indexTraverser,
                lookup,
                rowWriter)
            .DecorateWithPredefinedStreamLength(outputFile, inputFile.Length)
            .DecorateWithProgressRender(progressRenderer);

        return new CompositionRoot(
            buildIndexCommand,
            sortRowsCommand,
            [streamWriter, lookupStreamReader, inputFile, outputFile]);
    }

    public void Dispose()
    {
        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
    }
}