using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Loader;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SortTask.Adapter;
using SortTask.Application;
using SortTask.Domain;
using SortTask.Domain.RowGeneration;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SortTask.TestFileCreator;

// ReSharper disable once ClassNeverInstantiated.Global
public class CreateTestFileCommand : AsyncCommand<CreateTestFileCommand.Settings>
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class Settings : CommandSettings
    {
        [CommandOption("-f|--file")]
        [Description("Path to the file")]
        public string? FilePath { get; set; }

        [CommandOption("-s|--size")]
        [Description("File size in bytes")]
        public long FileSize { get; set; }

        [CommandOption("-h|--help")]
        [Description("Show help message")]
        public bool ShowHelp { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        const string usageMessage = "Usage: file-creator -f <file> -s <size>"; // TODO: fix file-creator app name

        if (settings.ShowHelp)
        {
            AnsiConsole.WriteLine(usageMessage);
            AnsiConsole.WriteLine("Options:");
            AnsiConsole.WriteLine("  -f, --file   Path to the file");
            AnsiConsole.WriteLine("  -s, --size   File size in bytes");
            AnsiConsole.WriteLine("  -h, --help   Show help message");
            return 0;
        }

        if (string.IsNullOrEmpty(settings.FilePath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] File path is required. {usageMessage}");
            return 1;
        }

        if (settings.FileSize <= 0)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Valid file size is required. {usageMessage}");
            return 1;
        }

        AnsiConsole.MarkupLine($"[green]Processing file:[/] {settings.FilePath.EscapeMarkup()}");
        AnsiConsole.MarkupLine($"[yellow]File size:[/] {settings.FileSize} bytes");

        var sc = BuildServiceCollection(settings.FilePath);
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

            // todo get required interface no class
            var fileRowFeeder = serviceProvider.GetRequiredService<FeedRowCommand>();
            await fileRowFeeder.Execute(settings.FileSize, cts.Token);
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
        const int maxRowNumber = 100_000;
        const int maxWordsInSentence = 5;
        const int repeatRowPeriod = 10;
        const int maxRepeatNumber = 1;
        const int refreshRepeatingRowsPeriod = 2;
        const int progressBarWidth = 50;

        var sc = new ServiceCollection();
        sc.AddSingleton<Stream>(_ => File.Create(filePath))
            .AddSingleton<Encoding>(_ => Encoding.UTF8)
            .AddSingleton<Random>()
            .AddSingleton<RandomRowGenerator>(
                sp => new RandomRowGenerator(
                    sp.GetRequiredService<Random>(),
                    maxRowNumber: maxRowNumber,
                    maxWordsInSentence: maxWordsInSentence))
            .AddSingleton<IRowGenerator>(sp =>
                new RowGenerationRepeater(
                    sp.GetRequiredService<RandomRowGenerator>(),
                    sp.GetRequiredService<Random>(),
                    repeatPeriod: repeatRowPeriod,
                    maxRepeatNumber: maxRepeatNumber,
                    refreshRepeatingRowsPeriod: refreshRepeatingRowsPeriod))
            .AddSingleton<IProgressRenderer>(_ => new ConsoleProgressRenderer(progressBarWidth))
            .AddSingleton<IRowWriter, StreamRowWriter>()
            .AddSingleton<FeedRowCommand>();

        return sc;
    }
}