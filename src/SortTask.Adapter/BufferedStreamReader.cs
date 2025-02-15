using System.Reflection;
using System.Text;

namespace SortTask.Adapter;

public class BufferedStreamReader(Stream stream, Encoding encoding) : IDisposable
{
    private static readonly FieldInfo CharPosField =
        typeof(StreamReader).GetField("_charPos",
            BindingFlags.NonPublic | BindingFlags.Instance |
            BindingFlags.DeclaredOnly)
        ?? throw new InvalidOperationException("_charPos field not found");

    private static readonly FieldInfo CharLenField =
        typeof(StreamReader).GetField("_charLen",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        ?? throw new InvalidOperationException("_charLen field not found");

    private static readonly FieldInfo CharBufferField =
        typeof(StreamReader)
            .GetField("_charBuffer", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        ?? throw new InvalidOperationException("_charBuffer field not found");

    private readonly StreamReader _streamReader = new(stream, encoding, leaveOpen: true);
    private long _offset;

    public void Dispose()
    {
        _streamReader.Dispose();
        GC.SuppressFinalize(this);
    }

    public ReadLineResult? ReadLine()
    {
        _offset = ActualPosition();
        var line = _streamReader.ReadLine();
        if (line == null) return null;

        var length = encoding.GetByteCount(line);
        var result = new ReadLineResult(line, _offset, length);
        return result;
    }

    private long ActualPosition()
    {
        var charBuffer = (char[])(CharBufferField.GetValue(_streamReader) ??
                                  throw new InvalidOperationException("_charBuffer value not found"));
        var charLen = (int)(CharLenField.GetValue(_streamReader) ??
                            throw new InvalidOperationException("_charLen value not found"));
        var charPos = (int)(CharPosField.GetValue(_streamReader) ??
                            throw new InvalidOperationException("_charPos value not found"));

        return _streamReader.BaseStream.Position -
               _streamReader.CurrentEncoding.GetByteCount(charBuffer, charPos, charLen - charPos);
    }

    public record ReadLineResult(string Line, long Offset, int Length);
}
