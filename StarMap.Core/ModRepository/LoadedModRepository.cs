using HarmonyLib;
using KSA;
using StarMap.API;
using System.Reflection;
using System.Runtime.Loader;

namespace StarMap.Core.ModRepository
{
    internal class LoadedModRepository : IDisposable
    {
        private readonly AssemblyLoadContext _coreAssemblyLoadContext;
        private readonly IEnumerable<Type> _registeredInterfaces = [];

        private readonly ModRegistry _mods = new();
        public ModRegistry Mods => _mods;

        public LoadedModRepository(AssemblyLoadContext coreAssemblyLoadContext)
        {
            _coreAssemblyLoadContext = coreAssemblyLoadContext;

            var baseInterface = typeof(IStarMapInterface);
            Assembly starMapTypes = baseInterface.Assembly;

            _registeredInterfaces = starMapTypes
                .GetTypes()
                .Where(
                    t => t.IsInterface &&
                    baseInterface.IsAssignableFrom(t) &&
                    t != baseInterface);
        }

        private bool IsStarMapMod(Type type)
        {
            return typeof(IStarMapMod).IsAssignableFrom(type) && !type.IsInterface;
        }

        public void LoadMod(Mod mod)
        {
            var fullPath = Path.GetFullPath(mod.DirectoryPath);
            var filePath = Path.Combine(fullPath, $"{mod.Name}.dll");
            var folderExists = Directory.Exists(fullPath);
            var fileExists = File.Exists(filePath);

            if (!folderExists || !fileExists) return;

            var modLoadContext = new ModAssemblyLoadContext(mod, _coreAssemblyLoadContext);
            var modAssembly = modLoadContext.LoadFromAssemblyName(new AssemblyName() { Name = mod.Name });

            var modClass = modAssembly.GetTypes().FirstOrDefault(IsStarMapMod);
            if (modClass is null) return;

            if (modClass.CreateInstance() is not IStarMapInterface modObject) return;

            foreach (var interfaceType in _registeredInterfaces)
            {
                if (interfaceType.IsAssignableFrom(modClass))
                {
                    _mods.Add(interfaceType, modObject);
                }
            }

            if (modObject is not IStarMapMod starMapMod) return;

            starMapMod.OnImmediatLoad();

            if (starMapMod.ImmediateUnload)
            {
                modLoadContext.Unload();
                return;
            }

            Console.WriteLine($"Loaded mod: {mod.Name}");
        }

        public void OnAllModsLoaded()
        {
            foreach (var mod in _mods.Get<IStarMapMod>())
            {
                mod.OnFullyLoaded();
            }
        }

        public void Dispose()
        {
            foreach (var mod in _mods.Get<IStarMapMod>())
            {
                mod.Unload();
            }

            _mods.Dispose();
        }
    }
}
