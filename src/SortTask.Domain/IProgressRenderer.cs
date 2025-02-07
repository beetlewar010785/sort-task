namespace SortTask.Domain;

public interface IProgressRenderer
{
    void Render(int percent, string text);
    void Complete();
}