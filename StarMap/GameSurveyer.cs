using StarMap.Types.Mods;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;

namespace StarMap
{
    internal class GameSurveyer : IDisposable
    {
        private readonly IGameFacade _facade;
        private readonly AssemblyLoadContext _gameAssemblyContext;
        private readonly string _gameLocation;

        private Assembly? _game;
        private IModManager? _modManager;

        public GameSurveyer(IGameFacade facade, AssemblyLoadContext alc, string location)
        {
            _facade = facade;
            _gameAssemblyContext = alc;
            _gameLocation = location;
        }

        public bool TryLoadModManagerAndGame([NotNullWhen(true)] out IModManager? modManager)
        {
            modManager = null;

            var modManagerAssembly = _gameAssemblyContext.LoadFromAssemblyPath(Path.GetFullPath("./StarMap.Core.dll"));

            var modManagerType = modManagerAssembly.GetTypes().FirstOrDefault((type) => typeof(IModManager).IsAssignableFrom(type) && !type.IsInterface);
            if (modManagerType is null) return false;
            var createdModManager = Activator.CreateInstance(modManagerType, [_gameAssemblyContext, _facade]);
            if (createdModManager is not IModManager manager) return false;

            _game = _gameAssemblyContext.LoadFromAssemblyPath(_gameLocation);

            _modManager = manager;
            manager.Init();
            modManager = manager;
            return true;
        }

        public void RunGame()
        {
            Debug.Assert(_game is not null, "Load needs to be called before running game");

            string[] args = [];
            _game.EntryPoint!.Invoke(null, [args]);
        }

        public void Dispose()
        {
            _gameAssemblyContext.Unload();
            _modManager?.DeInit();
        }
    }
}
