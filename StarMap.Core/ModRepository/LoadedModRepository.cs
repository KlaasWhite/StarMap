using KSA;
using StarMap.API;
using System.Reflection;
using System.Runtime.Loader;

namespace StarMap.Core.ModRepository
{
    internal class LoadedModRepository : IDisposable
    {
        private readonly AssemblyLoadContext _coreAssemblyLoadContext;
        private readonly Dictionary<string, StarMapMethodAttribute> _registeredMethodAttributes = [];

        private readonly ModRegistry _mods = new();
        public ModRegistry Mods => _mods;

        private (string attributeName, StarMapMethodAttribute attribute)? ConvertAttributeType(Type attrType)
        {
            if ((Activator.CreateInstance(attrType) as StarMapMethodAttribute) is not StarMapMethodAttribute attrObject) return null;
            return (attrType.Name, attrObject);
        }

        public LoadedModRepository(AssemblyLoadContext coreAssemblyLoadContext)
        {
            _coreAssemblyLoadContext = coreAssemblyLoadContext;

            Assembly coreAssembly = typeof(StarMapModAttribute).Assembly;

            _registeredMethodAttributes = coreAssembly
                .GetTypes()
                .Where(t =>
                    typeof(StarMapMethodAttribute).IsAssignableFrom(t) &&
                    t.IsClass &&
                    !t.IsAbstract &&
                    t.GetCustomAttribute<AttributeUsageAttribute>()?.ValidOn.HasFlag(AttributeTargets.Method) == true
                )
                .Select(ConvertAttributeType)
                .OfType<(string attributeName, StarMapMethodAttribute attribute)>()
                .ToDictionary();
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

            var modClass = modAssembly.GetTypes().FirstOrDefault(type => type.IsDefined(typeof(StarMapModAttribute), inherit: false));
            if (modClass is null) return;

            var modObject = Activator.CreateInstance(modClass);
            if (modObject is null) return;

            var classMethods = modClass.GetMethods();
            var immediateLoadMethods = new List<MethodInfo>();

            foreach (var classMethod in classMethods)
            {
                var stringAttrs = classMethod.GetCustomAttributes().Select((attr) => attr.GetType().Name).Where(_registeredMethodAttributes.Keys.Contains);
                foreach (var stringAttr in stringAttrs)
                {
                    var attr = _registeredMethodAttributes[stringAttr];

                    if (!attr.IsValidSignature(classMethod)) continue;

                    if (attr.GetType() == typeof(StarMapImmediateLoadAttribute))
                    {
                        immediateLoadMethods.Add(classMethod);
                    }

                    _mods.Add(attr, modObject, classMethod);
                }
            }

            foreach (var method in immediateLoadMethods)
            {
                method.Invoke(modObject, [mod]);
            }

            Console.WriteLine($"StarMap - Loaded mod: {mod.Name}");
        }

        public void OnAllModsLoaded()
        {
            foreach (var (_, @object, method) in _mods.Get<StarMapAllModsLoadedAttribute>())
            {
                method.Invoke(@object, []);
            }
        }

        public void Dispose()
        {
            foreach (var (_, @object, method) in _mods.Get<StarMapUnloadAttribute>())
            {
                method.Invoke(@object, []);
            }

            _mods.Dispose();
        }
    }
}
