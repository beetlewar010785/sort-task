using System.Reflection;
using SortTask.Checker;
using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp<CheckFileCommand>();
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