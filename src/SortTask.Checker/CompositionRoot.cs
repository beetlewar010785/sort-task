using SortTask.Adapter;
using SortTask.Application;
using SortTask.Application.Decorators;
using SortTask.Domain;

namespace SortTask.Checker;

public class CompositionRoot(
    ICommand<CheckSortCommand.Param, CheckSortCommand.Result> checkSortCommand,
    IList<IDisposable> disposables) : IDisposable
{
    public ICommand<CheckSortCommand.Param, CheckSortCommand.Result> CheckSortCommand => checkSortCommand;

    public static CompositionRoot Build(string filePath)
    {
        var file = File.OpenRead(filePath);

        var rowIterator = new StreamRowReadWriter(file, AdapterConst.Encoding);
        var command = new CheckSortCommand(
                rowIterator,
                new RowComparer())
            .DecorateWithStreamLength(file)
            .DecorateWithProgressRender(new ConsoleProgressRenderer(AdapterConst.ProgressBarWidth));

        return new CompositionRoot(command, [file]);
    }

    public void Dispose()
    {
        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
    }
}