using SortTask.TestFileCreator;
using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp<FileCommand>();
app.Configure(config =>
{
    config.SetApplicationName("SortTask.TestFileCreator");
    config.PropagateExceptions();
    config.SetExceptionHandler((ex, _) =>
    {
        AnsiConsole.MarkupLine("[red]An error occurred:[/] " + ex.Message);
        return 1;
    });
});

return app.Run(args);