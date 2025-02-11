using System.Reflection;
using System.Text;

namespace SortTask.Adapter;

public class BufferedStreamReader(Stream stream, Encoding encoding) : IDisposable
{
    private readonly StreamReader _streamReader = new(stream, encoding, leaveOpen: true);
    private long _offset;

    public record ReadLineResult(string Line, long Offset, long Length);

    public async Task<ReadLineResult?> ReadLine(CancellationToken cancellationToken)
    {
        _offset = ActualPosition();
        var line = await _streamReader.ReadLineAsync(cancellationToken);
        if (line == null) return null;

        var length = encoding.GetByteCount(line);
        var result = new ReadLineResult(line, _offset, length);
        return result;
    }

    public void Dispose() => _streamReader.Dispose();

    private static readonly FieldInfo CharPosField =
        typeof(StreamReader).GetField("_charPos",
            BindingFlags.NonPublic | BindingFlags.Instance |
            BindingFlags.DeclaredOnly) ??
        throw new Exception();

    private static readonly FieldInfo CharLenField =
        typeof(StreamReader).GetField("_charLen",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        ?? throw new Exception();

    private static readonly FieldInfo CharBufferField =
        typeof(StreamReader)
            .GetField("_charBuffer", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        ?? throw new Exception();

    private long ActualPosition()
    {
        var charBuffer = (char[])(CharBufferField.GetValue(_streamReader) ?? throw new Exception());
        var charLen = (int)(CharLenField.GetValue(_streamReader) ?? throw new Exception());
        var charPos = (int)(CharPosField.GetValue(_streamReader) ?? throw new Exception());

        return _streamReader.BaseStream.Position -
               _streamReader.CurrentEncoding.GetByteCount(charBuffer, charPos, charLen - charPos);
    }
}