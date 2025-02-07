namespace SortTask.Adapter.Test;

public class ConsoleProgressStringBuilderTests
{
    [TestCase(50, 000, "hello", "[                   hello   0%                   ]")]
    [TestCase(50, 030, "hello", "[■■■■■■■■■■■■■■     hello  30%                   ]")]
    [TestCase(50, 070, "hello", "[■■■■■■■■■■■■■■■■■■ hello  70% ■■■               ]")]
    [TestCase(50, 100, "hello", "[■■■■■■■■■■■■■■■■■■ hello 100% ■■■■■■■■■■■■■■■■■■]")]
    [TestCase(20, 100, "long-long-text-that-exceeds-progress-bar-width", "[ long-long-t 100% ]")]
    public void Should_Build_Expected_String(int barWidth, int percent, string sutText, string expectedString)
    {
        var sut = new ConsoleProgressStringBuilder(barWidth);

        var actualString = sut.BuildProgressString(percent, sutText);

        Assert.Multiple(() =>
        {
            Assert.That(actualString, Is.EqualTo(expectedString));
            Assert.That(actualString, Has.Length.EqualTo(barWidth));
        });
    }
}