using System.Reflection;
using SortTask.Sorter;
using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp<SortCommand>();
app.Configure(config =>
{
    config.SetApplicationName(Assembly.GetExecutingAssembly().GetName().FullName)
        .PropagateExceptions()
        .SetExceptionHandler((ex, _) =>
            {
                AnsiConsole.MarkupLine("[red]An error occurred:[/] " + ex.Message);
                return 1;
            }
        );
});

return app.Run(args);