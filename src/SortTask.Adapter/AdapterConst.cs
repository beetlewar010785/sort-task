using System.Text;

namespace SortTask.Adapter;

public static class AdapterConst
{
    public const string RowFieldsSplitter = ". ";
    public static readonly Encoding Encoding = Encoding.Unicode;
    public const int NumIndexOphWords = 4;
    public const int BTreeOrder = 10;
}