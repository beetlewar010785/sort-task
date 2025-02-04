namespace SortTask.Domain;

public interface IProgressRenderer
{
    void Render(int percent);
    void Clear();
}