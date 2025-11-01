using DummyProgram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

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
                Path.GetFullPath(Path.Combine(mod.DirectoryPath, mod.Assembly.Name + ".dll"))
            );
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var existingInDefault = Default.Assemblies
                .FirstOrDefault(a => a.FullName == assemblyName.FullName);
            if (existingInDefault != null)
                return existingInDefault;

            var existingInGameContext = _coreAssemblyLoadContext?.Assemblies
                .FirstOrDefault(a => a.FullName == assemblyName.FullName);
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
