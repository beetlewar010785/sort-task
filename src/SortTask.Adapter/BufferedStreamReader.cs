using System.Text;

namespace SortTask.Adapter;

public class BufferedStreamReader(Stream stream, Encoding encoding, int bufferSize = 1024)
{
    private readonly byte[] _buffer = new byte[bufferSize];
    private readonly Decoder _decoder = encoding.GetDecoder();
    private readonly char[] _charBuffer = new char[bufferSize];
    private int _bufferSize;
    private int _bufferPos;
    private long _byteOffset;

    public record ReadLineResult(string Line, long Offset, long Length);

    public async Task<ReadLineResult?> ReadLine(CancellationToken token)
    {
        var result = new StringBuilder();
        var foundNewLine = false;
        var startOffset = _byteOffset;
        var lineByteLength = 0L;

        var consumedBytes = 0;
        while (!foundNewLine)
        {
            if (_bufferPos >= _bufferSize)
            {
                _bufferSize = await stream.ReadAsync(_buffer, token);
                _bufferPos = 0;

                if (_bufferSize == 0)
                {
                    return result.Length > 0
                        ? new ReadLineResult(result.ToString(), startOffset, lineByteLength)
                        : null;
                }
            }

            var charCount = _decoder.GetChars(
                _buffer,
                _bufferPos,
                _bufferSize - _bufferPos,
                _charBuffer,
                0);

            for (var i = 0; i < charCount; i++)
            {
                var c = _charBuffer[i];

                var charSize = encoding.GetByteCount(new[] { c });
                consumedBytes += charSize;

                if (c == '\n')
                {
                    foundNewLine = true;
                    break;
                }

                if (c == '\r') continue;

                result.Append(c);
                lineByteLength += charSize;
            }

            _bufferPos += consumedBytes;
            _byteOffset += consumedBytes;
        }

        return new ReadLineResult(result.ToString(), startOffset, lineByteLength);
    }
}