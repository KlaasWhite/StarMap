
using KSA;
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
        private readonly AssemblyLoadContext _coreAssemblyLoadContext;
        private readonly IGameFacade _gameFacade;

        private readonly TaskCompletionSource<IPCGetCurrentManagedModsResponse> _managedMods = new();
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
            }

            _loadedMods.Clear();
        }

        public void LoadMod(Mod mod)
        {
            if (!Directory.Exists(mod.DirectoryPath)) return;

            var modLoadContext = new ModAssemblyLoadContext(mod, _coreAssemblyLoadContext);
            var modAssembly = modLoadContext.LoadFromAssemblyName(new AssemblyName() { Name = mod.Name });

            var loadedMod = modAssembly.GetTypes().FirstOrDefault((type) => typeof(IStarMapMod).IsAssignableFrom(type) && !type.IsInterface).CreateInstance();
            if (loadedMod is not IStarMapMod starMapMod) return;

            starMapMod.OnImmediatLoad();

            if (starMapMod.ImmediateUnload)
            {
                modLoadContext.Unload();
                return;
            }

            Console.WriteLine($"Loaded mod: {mod.Name}");

            _loadedMods[mod] = (starMapMod, modLoadContext);
            return;
        }

        public void OnAllModsLoaded()
        {
            foreach (var (mod, _) in _loadedMods.Values)
            {
                mod.OnFullyLoaded();
            }
        }

        public async Task RetrieveManagedMods()
        {
            var message = new IPCGetCurrentManagedModsRequest();

            var response = await _gameFacade.RequestData(message);

            if (!response.Is(IPCGetCurrentManagedModsResponse.Descriptor)) return;

            _managedMods.SetResult(response.Unpack<IPCGetCurrentManagedModsResponse>());
        }

        public IPCGetCurrentManagedModsResponse GetManagedMods()
        {
            return _managedMods.Task.GetAwaiter().GetResult();
        }

        public async Task<IPCMod[]> GetAvailableModsAsync()
        {
            var message = new IPCGetModsRequest();

            var response = await _gameFacade.RequestData(message);

            if (!response.Is(IPCGetModsResponse.Descriptor)) return[];

           return [.. response.Unpack<IPCGetModsResponse>().Mods];
        }

        public async Task<IPCModDetails?> GetModInformationAsync(string id)
        {
            var message = new IPCGetModDetailsRequest()
            {
                Id = id
            };

            var response = await _gameFacade.RequestData(message);

            if (!response.Is(IPCGetModDetailsResponse.Descriptor)) return null;

            return response.Unpack<IPCGetModDetailsResponse>().Mod;
        }

        public string[] GetLoadedMods()
        {
            if (_loadedMods is null) return [];

            return _loadedMods.Select(loadedMod => loadedMod.Key.Name).ToArray();
        }

        public async Task SetModUpdates(IPCSetManagedMods update)
        {
            await _gameFacade.RequestData(update);
        }
    }
}
