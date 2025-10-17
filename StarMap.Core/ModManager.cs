

using DummyProgram;
using Google.Protobuf.WellKnownTypes;
using HarmonyLib;
using StarMap.Types;
using StarMap.Types.Mods;
using StarMap.Types.Proto.IPC;
using System.Reflection;
using System.Runtime.Loader;

namespace StarMap.Core
{
    internal class ModManager : IModManager
    {
        private AssemblyLoadContext? _coreAssemblyLoadContext;
        private IGameFacade _gameFacade;

        private TaskCompletionSource<ManagedModsResponse> _managedMods = new();
        private readonly Dictionary<Mod, (IStarMapMod mod, ModAssemblyLoadContext assemblyContext)> _loadedMods = [];

        public ModManager(AssemblyLoadContext coreAssemblyLoadContext, IGameFacade gameFacade)
        {
            _coreAssemblyLoadContext = coreAssemblyLoadContext;
            _gameFacade = gameFacade;
        }

        public void Init()
        {
            _ = RetrieveManagedMods();
            ModLoaderPatcher.Patch(this);
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
            _coreAssemblyLoadContext = null;
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

        public async Task RetrieveManagedMods()
        {
            Console.WriteLine("RetrieveManagedMods");
            var message = new ManagedModsRequest();

            var response = await _gameFacade.RequestData(message);

            Console.WriteLine("RetrieveManagedMods 2");

            if (!response.Is(ManagedModsResponse.Descriptor)) return;

            _managedMods.SetResult(response.Unpack<ManagedModsResponse>());
        }

        public ManagedModsResponse GetManagedMods()
        {
            Console.WriteLine("GetManagedMods");
            return _managedMods.Task.GetAwaiter().GetResult();
        }

        public async Task<string[]> GetManagedModsAsync()
        {
            var message = new AvailableModsRequest();

            var response = await _gameFacade.RequestData(message);

            if (!response.Is(AvailableModsResponse.Descriptor)) return[];

           return [.. response.Unpack<AvailableModsResponse>().Mods];
        }

        public async Task<ModInformation?> GetModInformationAsync(string modName)
        {
            var message = new ModInformationRequest()
            {
                Mod = modName
            };

            var response = await _gameFacade.RequestData(message);

            if (!response.Is(ModInformationResponse.Descriptor)) return null;

            return response.Unpack<ModInformationResponse>().Mod;
        }

        public AssemblyName[] GetLoadedMods()
        {
            if (_loadedMods is null) return [];

            return _loadedMods.Select(loadedMod => loadedMod.Key.Assembly).ToArray();
        }

        public async Task SetModUpdates(SetModUpdates update)
        {
            await _gameFacade.RequestData(update);
        }
    }
}
