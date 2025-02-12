namespace SortTask.Application;

public interface IProgressRenderer
{
    void Render(int percent, string text);
    void Complete();
}