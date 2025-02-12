namespace SortTask.Adapter;

public class ConsoleProgressStringBuilder(int barWidth)
{
    public string BuildProgressString(int percent, string text)
    {
        const char progressSymbol = '■';
        const string openingSymbol = "[";
        const string closingSymbol = "]";

        if (percent is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(percent));
        }

        var openingAndClosingSymbols = openingSymbol.Length + closingSymbol.Length;
        var payloadWidth = barWidth - openingAndClosingSymbols;
        var percentSuffix = $" {percent,3}%";
        var progressText = new string(text
            .Take(payloadWidth - percentSuffix.Length - 2) // beginning and ending spaces + percent text
            .ToArray());
        progressText = $" {progressText}{percentSuffix} ";

        // calculate, how many progress symbols we should print
        var currentProgressWidth = payloadWidth * percent / 100;
        var maxAllowedSymbolsPerSide = (payloadWidth - progressText.Length) / 2;

        var numProgressSymbolsLeft = Math.Min(currentProgressWidth, maxAllowedSymbolsPerSide);
        var leftProgressSymbols = new string(progressSymbol, numProgressSymbolsLeft)
            .PadRight(maxAllowedSymbolsPerSide, ' ');

        var numSkipRight = progressText.Length / 2;
        var numProgressSymbolsRight = Math.Max(currentProgressWidth - payloadWidth / 2 - numSkipRight, 0);
        var rightProgressSymbols = new string(progressSymbol, numProgressSymbolsRight).PadRight(
            maxAllowedSymbolsPerSide, ' ');

        return $"{openingSymbol}{leftProgressSymbols}{progressText}{rightProgressSymbols}{closingSymbol}";
    }
}