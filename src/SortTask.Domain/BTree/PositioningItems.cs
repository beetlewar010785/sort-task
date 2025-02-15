namespace SortTask.Domain.BTree;

public readonly struct PositioningItems<T>(T[] values)
    where T : struct
{
    public IReadOnlyCollection<T> Values => values;

    public int Length => values.Length;

    public T this[int index] => values[index];

    public PositioningItems<T> Insert(T value, int position)
    {
        var newValues = new T[values.Length + 1];

        Array.Copy(values, 0, newValues, 0, position);
        newValues[position] = value;
        Array.Copy(values, position, newValues, position + 1, values.Length - position);

        return new PositioningItems<T>(newValues);
    }

    public int IndexOf(T value)
    {
        for (var i = 0; i < values.Length; i++)
            if (values[i].Equals(value))
                return i;

        return -1;
    }

    public int SearchPosition(T value, Comparison<T> comparer)
    {
        int left = 0, right = values.Length - 1;

        while (left <= right)
        {
            var mid = left + (right - left) / 2;
            var existingValue = values[mid];
            var compareResult = comparer(value, existingValue);

            switch (compareResult)
            {
                case 0:
                    return mid;
                case < 0:
                    right = mid - 1;
                    break;
                default:
                    left = mid + 1;
                    break;
            }
        }

        return left;
    }
}
