using SortTask.Adapter;
using SortTask.Application;
using SortTask.Application.Decorators;
using SortTask.Domain;

namespace SortTask.Checker;

public class CompositionRoot(
    ICommand<CheckSortCommand.Result> checkSortCommand,
    IList<IDisposable> disposables) : IDisposable
{
    public ICommand<CheckSortCommand.Result> CheckSortCommand => checkSortCommand;

    public void Dispose()
    {
        foreach (var disposable in disposables) disposable.Dispose();

        GC.SuppressFinalize(this);
    }

    public static CompositionRoot Build(string filePath)
    {
        var file = File.OpenRead(filePath);

        var rowIterator = new StreamRowStore(file, AdapterConst.Encoding);
        var command = new CheckSortCommand(
                rowIterator,
                new RowComparer())
            .DecorateWithStreamLength(file)
            .DecorateWithProgressRender(new ConsoleProgressRenderer());

        return new CompositionRoot(command, [file]);
    }
}
