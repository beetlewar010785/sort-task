using SortTask.Domain;

namespace SortTask.Adapter;

public class ConsoleProgressRenderer(int barWidth) : IProgressRenderer
{
    private string? _lastRenderedString;

    public void Render(int percent)
    {
        var halfBarWidth = barWidth / 2;
        const char progressSymbol = '■';

        if (percent is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(percent));
        }

        var progressBar = percent * barWidth / 100;

        var leftBarWidth = Math.Min(progressBar, halfBarWidth);
        var leftBarText = new string(progressSymbol, leftBarWidth).PadRight(halfBarWidth, ' ');

        var rightBarWidth = Math.Max(progressBar - halfBarWidth, 0);
        var rightBarText = new string(progressSymbol, rightBarWidth).PadRight(halfBarWidth, ' ');

        var percentText = $" {percent,3}% ";
        var progressBarText = $"[{leftBarText}{percentText}{rightBarText}]";
        Console.Write($"\r{progressBarText}");

        _lastRenderedString = progressBarText;
    }

    public void Clear()
    {
        if (_lastRenderedString == null) return;

        Console.Write("\r" + new string(' ', _lastRenderedString.Length) + "\r");
        _lastRenderedString = null;
    }
}