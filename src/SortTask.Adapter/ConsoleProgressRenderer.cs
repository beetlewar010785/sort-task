using SortTask.Application;

namespace SortTask.Adapter;

public class ConsoleProgressRenderer : IProgressRenderer
{
    private static readonly int WindowWidth = Math.Max(Console.WindowWidth, 50);

    private string? _lastRenderedString;
    private readonly ConsoleProgressStringBuilder _progressBuilder = new(WindowWidth);

    public void Render(int percent, string text)
    {
        var stringToRender = _progressBuilder.BuildProgressString(percent, text);
        if (stringToRender == _lastRenderedString)
        {
            return;
        }

        _lastRenderedString = stringToRender;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(_lastRenderedString.PadRight(WindowWidth));
    }

    public void Complete()
    {
        if (_lastRenderedString == null) return;

        Console.Write("\r" + new string(' ', _lastRenderedString.Length) + "\r");
        _lastRenderedString = null;
    }
}