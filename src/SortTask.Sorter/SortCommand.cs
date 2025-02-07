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
        [CommandOption("-u|--unsorted-file")]
        [Description("Path to the intput unsorted file")]
        public string? InputFilePath { get; set; }

        [CommandOption("-s|--sorted-file")]
        [Description("Path to the output sorted file")]
        public string? OutputFilePath { get; set; }

        [CommandOption("-o|--order")]
        [Description("BTree order")]
        public int BTreeOrder { get; set; }

        [CommandOption("-h|--help")]
        [Description("Show help message")]
        public bool ShowHelp { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        const string
            usageMessage =
                "Usage: sorter -u <unsorted-input-file> -s <sorted-output-file> -o <btree-order>"; // fix sorter app name

        if (settings.ShowHelp)
        {
            AnsiConsole.WriteLine(usageMessage);
            AnsiConsole.WriteLine("Options:");
            AnsiConsole.WriteLine("  -u, --unsorted-file  Path to the input unsorted file");
            AnsiConsole.WriteLine("  -s, --sorted-file    Path to the output sorted file");
            AnsiConsole.WriteLine("  -o, --order          BTree order");
            AnsiConsole.WriteLine("  -h, --help           Show help message");
            return 0;
        }

        if (string.IsNullOrEmpty(settings.InputFilePath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Input unsorted file path is required. {usageMessage}");
            return 1;
        }

        if (string.IsNullOrEmpty(settings.OutputFilePath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Output sorted file path is required. {usageMessage}");
            return 1;
        }

        if (settings.BTreeOrder <= 0)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] BTree order is required and must be > 0. {usageMessage}");
            return 1;
        }

        AnsiConsole.MarkupLine($"[yellow]Unsorted file:[/] {settings.InputFilePath.EscapeMarkup()}");
        AnsiConsole.MarkupLine($"[yellow]Sorted file: [/] {settings.OutputFilePath.EscapeMarkup()}");
        AnsiConsole.MarkupLine($"[yellow]BTree order: [/] {settings.BTreeOrder}");

        var sc = BuildServiceCollection(
            inputFilePath: settings.InputFilePath,
            outputFilePath: settings.OutputFilePath,
            bTreeOrder: settings.BTreeOrder
        );
        await using var serviceProvider = sc.BuildServiceProvider();

        AnsiConsole.MarkupLine("[green]Start sorting.[/]");

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

            var fileRowFeeder = serviceProvider.GetRequiredService<SortRowsCommand<MemoryBTreeIndex>>();
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

    private static ServiceCollection BuildServiceCollection(string inputFilePath, string outputFilePath, int bTreeOrder)
    {
        var sc = new ServiceCollection();

        var inputFile = File.OpenRead(inputFilePath);
        var outputFile = File.Create(outputFilePath);

        sc.AddSingleton<Encoding>(_ => Encoding.UTF8)
            .AddSingleton(inputFile)
            .AddSingleton(outputFile)
            .AddSingleton(_ => new BTreeOrder(bTreeOrder))
            .AddSingleton<IIndexer<MemoryBTreeIndex>,
                BTreeIndexer<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>>()
            .AddSingleton<IComparer<ReadRow>, RowComparer>()
            .AddSingleton<IRowReader, StreamRowReader>(sp =>
                new StreamRowReader(inputFile, sp.GetRequiredService<Encoding>()))
            .AddSingleton<IRowWriter, StreamRowWriter>(sp =>
                new StreamRowWriter(outputFile, sp.GetRequiredService<Encoding>()))
            .AddSingleton<IProgressRenderer>(_ => new ConsoleProgressRenderer(Const.ProgressBarWidth))
            .AddSingleton(sp => new SortRowsCommand<MemoryBTreeIndex>(
                sp.GetRequiredService<IRowReader>(),
                sp.GetRequiredService<IIndexer<MemoryBTreeIndex>>(),
                sp.GetRequiredService<IIndexFactory<MemoryBTreeIndex>>(),
                sp.GetRequiredService<IIndexTraverser<MemoryBTreeIndex>>(),
                sp.GetRequiredService<IRowLookup<MemoryBTreeIndex>>(),
                sp.GetRequiredService<IRowWriter>(),
                sp.GetRequiredService<IProgressRenderer>(),
                inputFile,
                outputFile));

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