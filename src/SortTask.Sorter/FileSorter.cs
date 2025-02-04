namespace SortTask.Sorter;

public class FileSorter(string fileName)
{
    public async Task Sort(CancellationToken cancellationToken)
    {
        await using var file = File.OpenRead(fileName);
        while (true)
        {
            
        }
    }
}