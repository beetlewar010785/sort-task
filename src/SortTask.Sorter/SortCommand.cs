using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Loader;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SortTask.Adapter;
using SortTask.Application;
using SortTask.Domain;
using SortTask.Domain.BTree;
using SortTask.Domain.BTree.Memory;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SortTask.Sorter;

// ReSharper disable once ClassNeverInstantiated.Global
public class SortCommand : AsyncCommand<SortCommand.Settings>
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class Settings : CommandSettings
    {
        [CommandOption("-i|--file-in")]
        [Description("Path to the intput file")]
        public string? InputFilePath { get; set; }

        [CommandOption("-o|--file-out")]
        [Description("Path to the output file")]
        public string? OutputFilePath { get; set; }

        [CommandOption("-h|--help")]
        [Description("Show help message")]
        public bool ShowHelp { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        const string usageMessage = "Usage: sorter -i <input-file> -o <output-file>";
        const int bTreeOrder = 1;

        if (settings.ShowHelp)
        {
            AnsiConsole.WriteLine(usageMessage);
            AnsiConsole.WriteLine("Options:");
            AnsiConsole.WriteLine("  -i, --file-in   Path to the input file");
            AnsiConsole.WriteLine("  -o, --file-out   Path to the output file");
            AnsiConsole.WriteLine("  -h, --help   Show help message");
            return 0;
        }

        if (string.IsNullOrEmpty(settings.InputFilePath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Input file path is required. {usageMessage}");
            return 1;
        }

        if (string.IsNullOrEmpty(settings.OutputFilePath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Output file path is required. {usageMessage}");
            return 1;
        }

        AnsiConsole.MarkupLine($"[green]Processing file:[/] {settings.InputFilePath.EscapeMarkup()}");

        var sc = BuildServiceCollection(
            inputFile: settings.InputFilePath,
            outputFile: settings.OutputFilePath,
            bTreeOrder: bTreeOrder
        );
        await using var serviceProvider = sc.BuildServiceProvider();

        var sw = new Stopwatch();
        sw.Start();

        try
        {
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += _ => { cts.Cancel(); };

            Console.CancelKeyPress += (_, eventArgs) =>
            {
                cts.Cancel();
                eventArgs.Cancel = true;
            };

            var fileRowFeeder = serviceProvider.GetRequiredService<ISortRowsCommand>();
            await fileRowFeeder.Execute(cts.Token);
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLine("[red]Operation was cancelled.[/]");
            return 1;
        }

        sw.Stop();

        AnsiConsole.MarkupLine($"[green]Operation completed successfully in {sw.Elapsed}.[/]");

        return 0;
    }

    private static ServiceCollection BuildServiceCollection(string inputFile, string outputFile, int bTreeOrder)
    {
        var sc = new ServiceCollection();

        sc.AddSingleton<Encoding>(_ => Encoding.UTF8)
            .AddSingleton(_ => new BTreeOrder(bTreeOrder))
            .AddSingleton<IIndexer<MemoryBTreeIndex>, BTreeIndexer<MemoryBTreeNode,
                MemoryBTreeIndex, MemoryBTreeNodeId>>()
            .AddSingleton<IComparer<Row>, RowComparer>()
            .AddSingleton<IRowReader, StreamRowReader>(sp =>
                new StreamRowReader(File.OpenRead(inputFile), sp.GetRequiredService<Encoding>()))
            .AddSingleton<IRowWriter, StreamRowWriter>(sp =>
                new StreamRowWriter(File.Create(outputFile), sp.GetRequiredService<Encoding>()))
            .AddSingleton<ISortRowsCommand, SortRowsCommand<MemoryBTreeIndex>>();

        AddMemoryServices(sc);

        return sc;
    }

    private static void AddMemoryServices(IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<IBTreeStore<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>,
                MemoryBTreeStore>()
            .AddSingleton<IBTreeNodeFactory<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>,
                MemoryBTreeNodeFactory>()
            .AddSingleton<IBTreeIndexComparer<MemoryBTreeIndex>,
                MemoryBTreeIndexComparer>()
            .AddSingleton<IIndexFactory<MemoryBTreeIndex>,
                MemoryBTreeIndexFactory>()
            .AddSingleton<IIndexTraverser<MemoryBTreeIndex>,
                BTreeIndexTraverser<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>>()
            .AddSingleton<IRowLookup<MemoryBTreeIndex>,
                MemoryBTreeRowLookup>();
    }
}