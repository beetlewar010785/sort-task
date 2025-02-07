using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Loader;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SortTask.Adapter;
using SortTask.Application;
using SortTask.Domain;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SortTask.Checker;

public class CheckFileCommand : AsyncCommand<CheckFileCommand.Settings>
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class Settings : CommandSettings
    {
        [CommandOption("-f|--file")]
        [Description("Path to the file to be checked")]
        public string? FilePath { get; set; }

        [CommandOption("-h|--help")]
        [Description("Show help message")]
        public bool ShowHelp { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        const string usageMessage = "Usage: checker -f <file>"; // TODO: fix app name

        if (settings.ShowHelp)
        {
            AnsiConsole.WriteLine(usageMessage);
            AnsiConsole.WriteLine("Options:");
            AnsiConsole.WriteLine("  -f, --file   Path to the file to be checked");
            AnsiConsole.WriteLine("  -h, --help   Show help message");
            return 0;
        }

        if (string.IsNullOrEmpty(settings.FilePath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] File path is required. {usageMessage}");
            return 1;
        }

        AnsiConsole.MarkupLine($"[yellow]Checking file:[/] {settings.FilePath.EscapeMarkup()}");

        var sc = BuildServiceCollection(settings.FilePath);
        await using var serviceProvider = sc.BuildServiceProvider();

        AnsiConsole.MarkupLine("[green]Start checking sorted file.[/]");

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

            var command = serviceProvider.GetRequiredService<CheckSortCommand>();
            var result = await command.Execute(cts.Token);
            if (result is CheckSortCommand.CheckSortResult.CheckSortResultFailure failure)
            {
                AnsiConsole.MarkupLine(
                    $"[red]File is not sorted. The row {failure.FailedRow} is less than the preceding row {failure.PrecedingRow}.[/]");
                return 1;
            }
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

    private static ServiceCollection BuildServiceCollection(string filePath)
    {
        var sc = new ServiceCollection();

        sc.AddSingleton<Encoding>(_ => Encoding.UTF8).AddSingleton<Stream>(File.OpenRead(filePath))
            .AddSingleton<IComparer<ReadRow>, RowComparer>()
            .AddSingleton<IRowReader, StreamRowReader>()
            .AddSingleton<IProgressRenderer>(_ => new ConsoleProgressRenderer(Const.ProgressBarWidth))
            .AddSingleton<CheckSortCommand>();

        return sc;
    }
}