using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Loader;
using SortTask.Adapter;
using SortTask.Domain.BTree;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SortTask.Sorter;

// ReSharper disable once ClassNeverInstantiated.Global
public class SortCommand : AsyncCommand<SortCommand.Settings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        const string usageMessage =
            "Usage: dotnet SortTask.Sorter -u <unsorted-input-file> -x <index-file> -s <sorted-output-file>";

        if (settings.ShowHelp)
        {
            AnsiConsole.WriteLine(usageMessage);
            AnsiConsole.WriteLine("Options:");
            AnsiConsole.WriteLine("  -u, --unsorted-file  Path to the input unsorted file");
            AnsiConsole.WriteLine("  -x, --index-file     Path to the index file");
            AnsiConsole.WriteLine("  -s, --sorted-file    Path to the output sorted file");
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

        AnsiConsole.MarkupLine($"[yellow]Unsorted file:[/] {settings.UnsortedFilePath.EscapeMarkup()}");
        AnsiConsole.MarkupLine($"[yellow]Sorted file: [/] {settings.SortedFilePath.EscapeMarkup()}");

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

            using var compositionRoot = CompositionRootBuilder.Build(
                settings.UnsortedFilePath,
                settings.IndexFilePath,
                settings.SortedFilePath,
                new BTreeOrder(AdapterConst.BTreeOrder),
                new Oph(AdapterConst.NumIndexOphWords));

            foreach (var initializer in compositionRoot.Initializers) await initializer.Initialize(cts.Token);

            await Execute(compositionRoot, cts.Token);

            AnsiConsole.MarkupLine(
                $"[yellow]Index collisions: [/] {compositionRoot.CollisionDetector.CollisionCount}");

            AnsiConsole.MarkupLine(
                $"[yellow]Index comparisons: [/] {compositionRoot.CollisionDetector.ComparisonCount}");

            AnsiConsole.MarkupLine(
                $"[yellow]Row lookup skip: [/] {compositionRoot.RowLookupCache.FindRowSkipCount}");

            AnsiConsole.MarkupLine(
                $"[yellow]Row lookup execute: [/] {compositionRoot.RowLookupCache.FindRowExecuteCount}");

            AnsiConsole.MarkupLine(
                $"[yellow]Get node skip: [/] {compositionRoot.BTreeStoreCache.GetNodeSkipCount}");

            AnsiConsole.MarkupLine(
                $"[yellow]Get node execute: [/] {compositionRoot.BTreeStoreCache.GetNodeExecuteCount}");
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

    private static async Task Execute<TOphValue>(CompositionRoot<TOphValue> compositionRoot, CancellationToken token)
        where TOphValue : struct
    {
        _ = await compositionRoot.BuildIndexCommand.Execute(token)
            .ToListAsync(token);

        _ = await compositionRoot.SortRowsCommand.Execute(token)
            .ToListAsync(token);
    }

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

        [CommandOption("-h|--help")]
        [Description("Show help message")]
        public bool ShowHelp { get; set; }
    }
}
