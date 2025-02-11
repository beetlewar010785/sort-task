using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Loader;
using SortTask.Adapter.StreamBTree;
using SortTask.Application;
using SortTask.Domain.BTree;
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
        [Description("Path to the input unsorted file")]
        public string? UnsortedFilePath { get; set; }

        [CommandOption("-x|--index-file")]
        [Description("Path to the index file")]
        public string? IndexFilePath { get; set; }

        [CommandOption("-s|--sorted-file")]
        [Description("Path to the output sorted file")]
        public string? SortedFilePath { get; set; }

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
                "Usage: sorter -u <unsorted-input-file> -x <index-file> -s <sorted-output-file> -o <btree-order>"; // fix sorter app name

        if (settings.ShowHelp)
        {
            AnsiConsole.WriteLine(usageMessage);
            AnsiConsole.WriteLine("Options:");
            AnsiConsole.WriteLine("  -u, --unsorted-file  Path to the input unsorted file");
            AnsiConsole.WriteLine("  -x, --index-file     Path to the index file");
            AnsiConsole.WriteLine("  -s, --sorted-file    Path to the output sorted file");
            AnsiConsole.WriteLine("  -o, --order          BTree order");
            AnsiConsole.WriteLine("  -h, --help           Show help message");
            return 0;
        }

        if (string.IsNullOrEmpty(settings.UnsortedFilePath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Input unsorted file path is required. {usageMessage}");
            return 1;
        }

        if (string.IsNullOrEmpty(settings.IndexFilePath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Index file path is required. {usageMessage}");
            return 1;
        }

        if (string.IsNullOrEmpty(settings.SortedFilePath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Output sorted file path is required. {usageMessage}");
            return 1;
        }

        if (settings.BTreeOrder <= 0)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] BTree order is required and must be > 0. {usageMessage}");
            return 1;
        }

        AnsiConsole.MarkupLine($"[yellow]Unsorted file:[/] {settings.UnsortedFilePath.EscapeMarkup()}");
        AnsiConsole.MarkupLine($"[yellow]Sorted file: [/] {settings.SortedFilePath.EscapeMarkup()}");
        AnsiConsole.MarkupLine($"[yellow]BTree order: [/] {settings.BTreeOrder}");

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

            using var compositionRoot = CompositionRoot.Build(
                unsortedFilePath: settings.UnsortedFilePath,
                indexFilePath: settings.IndexFilePath,
                sortedFilePath: settings.SortedFilePath,
                order: new BTreeOrder(settings.BTreeOrder));

            foreach (var initializer in compositionRoot.Initializers)
            {
                await initializer.Initialize(cts.Token);
            }

            await compositionRoot.BuildIndexCommand.Execute(new BuildIndexCommand.Param(), cts.Token)
                .ToListAsync(cancellationToken: cts.Token);

            await compositionRoot.SortRowsCommand.Execute(new SortRowsCommand.Param(), cts.Token)
                .ToListAsync(cancellationToken: cts.Token);

            AnsiConsole.MarkupLine(
                $"[yellow]Index collision number: [/] {compositionRoot.CollisionDetector.CollisionNumber}");

            AnsiConsole.MarkupLine(
                $"[yellow]Row lookup number: [/] {compositionRoot.RowLookupCounter.Count}");
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
}