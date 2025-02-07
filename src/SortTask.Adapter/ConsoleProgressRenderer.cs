using SortTask.Domain;

namespace SortTask.Adapter;

public class ConsoleProgressRenderer(int barWidth) : IProgressRenderer
{
    private string? _lastRenderedString;
    private readonly ConsoleProgressStringBuilder _progressBuilder = new ConsoleProgressStringBuilder(barWidth);

    public void Render(int percent, string text)
    {
        _lastRenderedString = _progressBuilder.BuildProgressString(percent, text);
        Console.Write($"\r{_lastRenderedString}");
    }

    public void Clear()
    {
        if (_lastRenderedString == null) return;

        Console.Write("\r" + new string(' ', _lastRenderedString.Length) + "\r");
        _lastRenderedString = null;
    }
}