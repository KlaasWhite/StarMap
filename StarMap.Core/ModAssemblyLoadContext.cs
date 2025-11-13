using KSA;
using System.Reflection;
using System.Runtime.Loader;

namespace StarMap.Core
{
    internal class ModAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyLoadContext _coreAssemblyLoadContext;
        private readonly AssemblyDependencyResolver _modDependencyResolver;

        public ModAssemblyLoadContext(Mod mod, AssemblyLoadContext coreAssemblyContext)
            : base(isCollectible: true)
        {
            _coreAssemblyLoadContext = coreAssemblyContext;

            _modDependencyResolver = new AssemblyDependencyResolver(
                Path.GetFullPath(Path.Combine(mod.DirectoryPath, mod.Name + ".dll"))
            );
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var existingInDefault = Default.Assemblies
                .FirstOrDefault(a => string.Equals(a.GetName().Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase));
            if (existingInDefault != null)
                return existingInDefault;

            var existingInGameContext = _coreAssemblyLoadContext?.Assemblies
                .FirstOrDefault(a => string.Equals(a.GetName().Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase));
            if (existingInGameContext != null)
                return existingInGameContext;

            var foundPath = _modDependencyResolver.ResolveAssemblyToPath(assemblyName);
            if (foundPath is null)
                return null;

            var path = Path.GetFullPath(foundPath);
            return path != null ? LoadFromAssemblyPath(path) : null;
        }
    }
}
