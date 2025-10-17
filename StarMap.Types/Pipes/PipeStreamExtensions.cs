using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System.IO.Pipes;

namespace StarMap.Types.Pipes
{
    internal static class PipeStreamExtensions
    {
        public static async Task<Any> ReadProtoAsync(this PipeStream pipeStream, CancellationToken cancellationToken = default)
        {
            // Read length prefix
            byte[] lengthPrefix = new byte[4];
            int read = await pipeStream.ReadAsync(lengthPrefix, 0, 4, cancellationToken);
            if (read != 4)
                throw new Exception("Failed to read message length");

            int messageLength = BitConverter.ToInt32(lengthPrefix, 0);

            // Read the actual message bytes
            byte[] buffer = new byte[messageLength];
            int totalRead = 0;
            while (totalRead < messageLength)
            {
                int bytesRead = await pipeStream.ReadAsync(buffer, totalRead, messageLength - totalRead, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return new Any();
                if (bytesRead == 0)
                    throw new Exception("Pipe closed before reading full message");
                totalRead += bytesRead;
            }

            return Any.Parser.ParseFrom(buffer);
        }

        public static async Task WriteProtoAsync(this PipeStream pipeStream, IMessage message, CancellationToken cancellationToken = default)
        {
            byte[] data = message.ToByteArray();

            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
            await pipeStream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length, cancellationToken);

            await pipeStream.WriteAsync(data, 0, data.Length, cancellationToken);
            pipeStream.Flush();
        }
    }
}
