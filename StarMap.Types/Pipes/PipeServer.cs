using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System.IO.Pipes;

namespace StarMap.Types.Pipes
{
    public sealed class PipeServer : IDisposable
    {
        public string PipeName { get; }
        private NamedPipeServerStream? _server;

        private Task? _readingTask;
        private CancellationTokenSource? _readingCts;
        public event EventHandler<Any>? OnMessage;

        public PipeServer(string pipeName)
        {
            PipeName = pipeName;
        }

        public async Task StartListening(CancellationToken cancellationToken)
        {
            // Create a named pipe server with bidirectional capability
            _server = new NamedPipeServerStream(
                PipeName,
                PipeDirection.InOut,
                maxNumberOfServerInstances: 1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            await _server.WaitForConnectionAsync(cancellationToken);

            _readingCts = new CancellationTokenSource();
            _readingTask = Task.Run(() => MessageReaderThread(_readingCts.Token), cancellationToken);
        }

        private async Task MessageReaderThread(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_server is null)
                    break;

                Any? message = null;

                try
                {
                    message = await _server.ReadProtoAsync(cancellationToken);
                }
                catch {}
                
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (message is null)
                    continue;

                _ = Task.Run(() => OnMessage?.Invoke(this, message), cancellationToken);
            }
        }

        public async Task SendResponseAsync(IMessage message, CancellationToken cancellationToken = default)
        {
            if (_server is null)
                return;

            await _server.WriteProtoAsync(message, cancellationToken);
        }

        public void Stop()
        {
            _readingCts?.Cancel();
            _readingTask?.Wait(TimeSpan.FromSeconds(5));
            _server?.Dispose();
            _server = null;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
