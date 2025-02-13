namespace SortTask.Application;

public record CommandIteration<TResult>(
    TResult? Result,
    string OperationName,
    int? ProgressPercent = null)
{
    public CommandIteration<TResult> SetProgress(int progressPercent)
    {
        return this with { ProgressPercent = progressPercent };
    }
}
