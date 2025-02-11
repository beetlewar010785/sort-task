using SortTask.Domain;

namespace SortTask.Adapter;

public class ConsoleProgressRenderer(int barWidth) : IProgressRenderer
{
    private static readonly TimeSpan RenderInterval = TimeSpan.FromMilliseconds(200);
    
    private string? _lastRenderedString;
    private readonly ConsoleProgressStringBuilder _progressBuilder = new(barWidth);
    private DateTime _lastRenderTime;

    public void Render(int percent, string text)
    {
        var now = DateTime.Now;
        var elapsed = now - _lastRenderTime;
        if (elapsed < RenderInterval)
        {
            return;
        }
        
        _lastRenderedString = _progressBuilder.BuildProgressString(percent, text);
        Console.Write($"\r{_lastRenderedString}");
        _lastRenderTime = now;
    }

    public void Complete()
    {
        if (_lastRenderedString == null) return;

        Console.Write("\r" + new string(' ', _lastRenderedString.Length) + "\r");
        _lastRenderedString = null;
    }
}