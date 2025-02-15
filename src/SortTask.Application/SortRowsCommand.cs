using System.Runtime.CompilerServices;
using SortTask.Domain;
using SortTask.Domain.BTree;

namespace SortTask.Application;

public class SortRowsCommand<TOphValue>(
    IBTreeIndexTraverser<TOphValue> indexTraverser,
    IRowLookup rowLookup,
    IRowWriter outputRowWriter
) : ICommand<SortRowsCommand<TOphValue>.Result> where TOphValue : struct
{
    public IEnumerable<CommandIteration<Result>> Execute()
    {
        const string operationName = "Sorting...";

        foreach (var index in indexTraverser.IterateOverIndex())
        {
            var row = rowLookup.FindRow(index.Offset, index.Length);
            outputRowWriter.Write(row);
            yield return new CommandIteration<Result>(null, operationName);
        }

        outputRowWriter.Flush();
    }

    public abstract record Result;
}
