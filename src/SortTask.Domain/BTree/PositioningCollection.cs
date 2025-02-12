namespace SortTask.Domain.BTree;

public readonly struct PositioningCollection<T>(T[] values)
    where T : struct
{
    public IReadOnlyCollection<T> Values => values;

    public int Length => values.Length;
    
    public T this[int index] => values[index];
    
    public PositioningCollection<T> Insert(T value, int position)
    {
        var newValues = new T[values.Length + 1];

        Array.Copy(values, 0, newValues, 0, position);
        newValues[position] = value;
        Array.Copy(values, position, newValues, position + 1, values.Length - position);

        return new PositioningCollection<T>(newValues);
    }

    public int IndexOf(T value)
    {
        for (var i = 0; i < values.Length; i++)
        {
            if (values[i].Equals(value))
            {
                return i;
            }
        }

        return -1;
    }
}