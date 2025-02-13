using SortTask.Application;

namespace SortTask.Adapter;

public class ConsoleProgressRenderer : IProgressRenderer
{
    private static readonly int WindowWidth = Math.Max(Console.WindowWidth, 50);
    private readonly ConsoleProgressStringBuilder _progressBuilder = new(WindowWidth);

    private string? _lastRenderedString;

    public void Render(int percent, string text)
    {
        var stringToRender = _progressBuilder.BuildProgressString(percent, text);
        if (stringToRender == _lastRenderedString) return;

        _lastRenderedString = stringToRender;
        Console.Write($"\r{_lastRenderedString}");
    }

    public void Complete()
    {
        if (_lastRenderedString == null) return;

        Console.Write("\r" + new string(' ', _lastRenderedString.Length) + "\r");
        _lastRenderedString = null;
    }
}
