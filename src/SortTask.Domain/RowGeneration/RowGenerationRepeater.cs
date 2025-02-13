namespace SortTask.Domain.RowGeneration;

public class RowGenerationRepeater(
    IRowGenerator inner,
    Random rnd,
    int repeatPeriod = 1000,
    int maxRepeatNumber = 2,
    int refreshRepeatingRowsPeriod = 2
) : IRowGenerator
{
    private readonly List<Row> _repeatingRows = [];

    public IEnumerable<Row> Generate()
    {
        var rows = inner.Generate().ToList();
        var repeatValue = rnd.Next(1, repeatPeriod);

        if (repeatValue == 1 && _repeatingRows.Count > 0)
        {
            var repeatNumber = rnd.Next(1, maxRepeatNumber);
            for (var i = 0; i < repeatNumber; i++) rows.AddRange(_repeatingRows);

            _repeatingRows.Clear();
        }

        var refreshRepeatingRowsPeriodValue = rnd.Next(1, refreshRepeatingRowsPeriod);
        if (refreshRepeatingRowsPeriodValue == 1)
        {
            _repeatingRows.Clear();
            _repeatingRows.AddRange(rows);
        }

        return rows;
    }
}
