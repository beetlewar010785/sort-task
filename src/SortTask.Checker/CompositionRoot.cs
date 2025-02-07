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

        var streamReader = new StreamReader(file, AdapterConst.Encoding, leaveOpen: true);
        var rowReader = new StreamRowReader(streamReader);
        var command = new CheckSortCommand(
                rowReader,
                new RowComparer())
            .DecorateWithStreamLength(file)
            .DecorateWithProgressRender(new ConsoleProgressRenderer(AdapterConst.ProgressBarWidth));

        return new CompositionRoot(command, [streamReader, file]);
    }

    public void Dispose()
    {
        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
    }
}