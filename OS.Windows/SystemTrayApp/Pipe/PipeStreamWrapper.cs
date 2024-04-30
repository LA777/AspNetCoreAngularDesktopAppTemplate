using System.IO;
using System.Text;

namespace SystemTrayApp.Pipe
{
    public class PipeStreamWrapper
    {
        private readonly Stream _ioStream;
        private readonly UnicodeEncoding _streamEncoding;

        public PipeStreamWrapper(Stream ioStream)
        {
            _ioStream = ioStream ?? throw new ArgumentNullException(nameof(ioStream));
            _streamEncoding = new UnicodeEncoding();
        }

        public async Task<string> ReadStringAsync(CancellationToken cancellationToken)
        {
            var bufferLength = _ioStream.ReadByte() * 256;
            bufferLength += _ioStream.ReadByte();
            var inputBuffer = new byte[bufferLength];

            _ = await _ioStream.ReadAsync(inputBuffer, 0, bufferLength, cancellationToken);
            var decodedText = _streamEncoding.GetString(inputBuffer);

            return decodedText;
        }

        public async Task<int> WriteStringAsync(string outputString, CancellationToken cancellationToken)
        {
            var outputBuffer = _streamEncoding.GetBytes(outputString);
            var bufferLength = outputBuffer.Length;
            if (bufferLength > ushort.MaxValue)
            {
                bufferLength = ushort.MaxValue;
            }
            _ioStream.WriteByte((byte)(bufferLength / 256));
            _ioStream.WriteByte((byte)(bufferLength & 255));
            await _ioStream.WriteAsync(outputBuffer, 0, bufferLength, cancellationToken);
            await _ioStream.FlushAsync(cancellationToken);

            return outputBuffer.Length + 2;
        }
    }
}
