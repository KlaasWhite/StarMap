using StarMap.Types.Mods;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;

namespace StarMap
{
    internal class GameSurveyer : IDisposable
    {
        private readonly GameFacade _facade;
        private readonly AssemblyLoadContext _gameAssemblyContext;
        private readonly string _gameLocation;

        private Assembly? _game;
        private IModManager? _modManager;

        public GameSurveyer(GameFacade facade, AssemblyLoadContext alc, string location)
        {
            _facade = facade;
            _gameAssemblyContext = alc;
            _gameLocation = location;
        }

        public IModManager? LoadModManagerAndGame()
        {
            var modManagerAssembly = _gameAssemblyContext.LoadFromAssemblyPath(Path.GetFullPath("./StarMap.Core.dll"));

            var modManagerType = modManagerAssembly.GetTypes().FirstOrDefault((type) => typeof(IModManager).IsAssignableFrom(type) && !type.IsInterface);
            if (modManagerType is null) return null;
            var createdModManager = Activator.CreateInstance(modManagerType, [_gameAssemblyContext, _facade]);
            if (createdModManager is not IModManager modManager) return null;

            _game = _gameAssemblyContext.LoadFromAssemblyPath(Path.Combine(_gameLocation, "DummyProgram.dll"));

            modManager.Init();
            return modManager;
        }

        public Task RunGame()
        {
            Debug.Assert(_game is not null, "Load needs to be called before running game");

            var gameExitedSources = new TaskCompletionSource();

            _ = Task.Run(() =>
            {
                string[] args = [];
                _game.EntryPoint!.Invoke(null, [args]);
                gameExitedSources.TrySetResult();
            });

            return gameExitedSources.Task;
        }

        public void Dispose()
        {
            _gameAssemblyContext.Unload();
            _modManager?.DeInit();
        }
    }
}
