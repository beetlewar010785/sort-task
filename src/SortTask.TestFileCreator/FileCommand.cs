using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using SortTask.Adapter;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SortTask.TestFileCreator;

// ReSharper disable once ClassNeverInstantiated.Global
public class FileCommand : AsyncCommand<FileCommand.Settings>
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

        var sw = new Stopwatch();
        sw.Start();

        var fileCreator = new FeedRowCommand(settings.FilePath, settings.FileSize);
        await fileCreator.Execute(CancellationToken.None); // todo fix none

        sw.Stop();

        AnsiConsole.MarkupLine($"[green]Operation completed successfully in {sw.Elapsed}.[/]");
        return 0;
    }
}