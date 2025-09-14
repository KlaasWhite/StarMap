

using DummyProgram;
using HarmonyLib;
using StarMap.Core.Types;
using StarMap.Types;
using System.Reflection;
using System.Runtime.Loader;

namespace StarMap.Core
{
    internal class ModManager : IModManager
    {
        private AssemblyLoadContext? _coreAssemblyLoadContext;
        private IModRepository? _modRepository;

        private Dictionary<Mod, (IStarMapMod mod, ModAssemblyLoadContext assemblyContext)>? _loadedMods = [];

        public ModManager(AssemblyLoadContext coreAssemblyLoadContext, IModRepository modRepository)
        {
            _coreAssemblyLoadContext = coreAssemblyLoadContext;
            _modRepository = modRepository;
        }

        public void Init()
        {
            ModLoaderPatcher.Patch(this, _modRepository);
        }

        public void DeInit() {
            ModLoaderPatcher.Unload();

            foreach (var mod in _loadedMods.Values)
            {
                mod.mod.Unload();
                mod.assemblyContext.Dispose();
                mod.assemblyContext.Unload();
            }

            _loadedMods.Clear();
            _loadedMods = null;
            _coreAssemblyLoadContext = null;
            _modRepository = null;
        }

        public void LoadMod(Mod mod)
        {
            if (!Directory.Exists(mod.DirectoryPath)) return;
            var modLoadContext = new ModAssemblyLoadContext(mod, _coreAssemblyLoadContext);
            var modAssembly = modLoadContext.LoadFromAssemblyName(mod.Assembly);

            var loadedMod = modAssembly.GetTypes().FirstOrDefault((type) => typeof(IStarMapMod).IsAssignableFrom(type) && !type.IsInterface).CreateInstance();
            if (loadedMod is not IStarMapMod KWMod) return;

            KWMod.OnImmediatLoad();

            if (KWMod.ImmediateUnload)
            {
                modLoadContext.Unload();
                return;
            }

            _loadedMods[mod] = (KWMod, modLoadContext);
            return;
        }

        public void OnAllModsLoaded()
        {
            foreach (var (mod, context) in _loadedMods.Values)
            {
                mod.OnFullyLoaded();
            }
        }

        public AssemblyName[] GetLoadedMods()
        {
            return _loadedMods.Select(loadedMod => loadedMod.Key.Assembly).ToArray();
        }
    }
}
