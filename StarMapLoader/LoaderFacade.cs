using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using StarMap.Types.Pipes;
using StarMap.Types.Proto.IPC;

namespace StarMapLoader
{
    internal class LoaderFacade : IDisposable
    {
        private readonly PipeServer _pipeServer;
        private readonly LoaderConfig _config;
        private readonly ModRepository _repository;

        public event EventHandler? OnProcessStarted;

        public LoaderFacade(PipeServer pipeServer, LoaderConfig config, ModRepository repository)
        {
            _pipeServer = pipeServer;
            _config = config;
            _repository = repository;

            _pipeServer.OnMessage += PipeServer_OnRequest;
        }

        private void PipeServer_OnRequest(object? sender, Any pipeMessage)
        {
            if (!pipeMessage.Is(PipeMessage.Descriptor)) return;

            var unpackedPipeMessage = pipeMessage.Unpack<PipeMessage>(); 
            var message = unpackedPipeMessage.Payload;

            IMessage? response = null;

            if (message.Is(ConnectRequest.Descriptor))
            {
                response = new ConnectResponse()
                {
                    GameLocation = _config.GamePath,
                };

                OnProcessStarted?.Invoke(this, EventArgs.Empty);
            }
            if (message.Is(ManagedModsRequest.Descriptor))
            {
                var modsRepsonse = new ManagedModsResponse();
                modsRepsonse.Mods.AddRange(_repository.LoadedModInformation);
                response = modsRepsonse;
            }

            if (message.Is(AvailableModsRequest.Descriptor))
            {
                var availableModsResponse = new AvailableModsResponse();
                availableModsResponse.Mods.AddRange(_repository.GetPossibleMods());
                response = availableModsResponse;
            }

            if (message.Is(ModInformationRequest.Descriptor))
            {
                var modInformationRequest = message.Unpack<ModInformationRequest>();

                var modInformationResponse = new ModInformationResponse()
                {
                    Mod = _repository.GetModInformation(modInformationRequest.Mod)
                };
                response = modInformationResponse;
            }

            if (message.Is(SetModUpdates.Descriptor))
            {
                var setModUpdates = message.Unpack<SetModUpdates>();

                _repository.SetModUpdates([.. setModUpdates.Updates]);

                response = new Any();
            }

            if (response is not null)
            {
                _pipeServer.SendResponseAsync(Any.Pack(new PipeMessage()
                {
                    RequestId = unpackedPipeMessage.RequestId,
                    Payload = Any.Pack(response)
                })).GetAwaiter().GetResult();
            }
        }

        public void Dispose()
        {
            _pipeServer.OnMessage -= PipeServer_OnRequest;
        }
    }
}
