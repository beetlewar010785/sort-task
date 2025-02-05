namespace SortTask.Domain;

// public class RowIndexKey(string sentencePart)
// {
//     private const int MaxSentencePartLength = 4;
//
//     public string SentencePart { get; } = sentencePart;
//
//     public static RowIndexKey FromRow(Row row)
//     {
//         return new RowIndexKey(new string(row.Sentence.Take(MaxSentencePartLength).ToArray()));
//     }
//
//     public override string ToString()
//     {
//         return SentencePart;
//     }
// }