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
        [CommandOption("-f|--file")]
        [Description("Path to the file")]
        public string? FilePath { get; set; }

        [CommandOption("-h|--help")]
        [Description("Show help message")]
        public bool ShowHelp { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        const string usageMessage = "Usage: sorter -f <file>"; // TODO: fix sorter app name
        const int bTreeOrder = 1;

        if (settings.ShowHelp)
        {
            AnsiConsole.WriteLine(usageMessage);
            AnsiConsole.WriteLine("Options:");
            AnsiConsole.WriteLine("  -f, --file   Path to the file");
            AnsiConsole.WriteLine("  -h, --help   Show help message");
            return 0;
        }

        if (string.IsNullOrEmpty(settings.FilePath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] File path is required. {usageMessage}");
            return 1;
        }

        AnsiConsole.MarkupLine($"[green]Processing file:[/] {settings.FilePath.EscapeMarkup()}");

        var sc = BuildServiceCollection(settings.FilePath, bTreeOrder);
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

            var fileRowFeeder = serviceProvider.GetRequiredService<SortRowsCommand<StreamReadRow>>();
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

    private static ServiceCollection BuildServiceCollection(string filePath, int bTreeOrder)
    {
        var sc = new ServiceCollection();

        sc.AddSingleton<Stream>(_ => File.OpenRead(filePath))
            .AddSingleton<Encoding>(_ => Encoding.UTF8)
            .AddSingleton(_ => new BTreeOrder(bTreeOrder))
            .AddSingleton<IRowReader<StreamReadRow>, StreamRowReader>()
            .AddSingleton<SortRowsCommand<StreamReadRow>>();

        return sc;
    }
}