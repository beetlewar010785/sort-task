using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Loader;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SortTask.TestFileCreator;

// ReSharper disable once ClassNeverInstantiated.Global
public class CreateTestFileCommand : AsyncCommand<CreateTestFileCommand.Settings>
{
    public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        const string usageMessage = "Usage: dotnet SortTask.TestFileCreator.dll -f <file> -s <size>";

        if (settings.ShowHelp)
        {
            AnsiConsole.WriteLine(usageMessage);
            AnsiConsole.WriteLine("Options:");
            AnsiConsole.WriteLine("  -f, --file   Path to the file");
            AnsiConsole.WriteLine("  -s, --size   File size in bytes");
            AnsiConsole.WriteLine("  -h, --help   Show help message");
            return Task.FromResult(0);
        }

        if (string.IsNullOrEmpty(settings.FilePath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] File path is required. {usageMessage}");
            return Task.FromResult(1);
        }

        if (settings.FileSize <= 0)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Valid file size is required. {usageMessage}");
            return Task.FromResult(1);
        }

        AnsiConsole.MarkupLine($"[yellow]Generating file:[/] {settings.FilePath.EscapeMarkup()}");
        AnsiConsole.MarkupLine($"[yellow]File size:[/] {settings.FileSize} bytes");

        AnsiConsole.MarkupLine("[green]Start generating test file.[/]");

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

            using var compositionRoot = CompositionRoot.Build(settings.FilePath, settings.FileSize);

            foreach (var _ in compositionRoot.FeedRowCommand.Execute())
            {
                cts.Token.ThrowIfCancellationRequested();
            }
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLine("[red]Operation was cancelled.[/]");
            return Task.FromResult(1);
        }

        sw.Stop();

        AnsiConsole.MarkupLine($"[green]Operation completed successfully in {sw.Elapsed}.[/]");
        return Task.FromResult(0);
    }

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
}
