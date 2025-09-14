using StarMap.Core.Types;
using System.Runtime.Loader;

namespace StarMap
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var gamePath = Path.GetFullPath("./");

            var modRepository = new ModRepository(Path.Combine(gamePath, "mods"));

            var shouldReload = true;

            while (shouldReload)
            {
                shouldReload = false;

                RunGame(gamePath, modRepository, args);

                for (int i = 0; i < 100; i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                Console.ReadLine();

                shouldReload = modRepository.HasChanges;
                modRepository.ApplyModUpdates();
            }
        }

        static void RunGame(string gamePath, ModRepository modRepository, string[] args)
        {
            var gameAssemblyContext = new CoreAssemblyLoadContext(gamePath);

            AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath("./0Harmony.dll"));
            var modManagerAssembly = gameAssemblyContext.LoadFromAssemblyPath(Path.GetFullPath("./StarMap.Core.dll"));

            var modManagerType = modManagerAssembly.GetTypes().FirstOrDefault((type) => typeof(IModManager).IsAssignableFrom(type) && !type.IsInterface);
            if (modManagerType is null) return;
            var createdModManager = Activator.CreateInstance(modManagerType, [gameAssemblyContext, modRepository]);
            if (createdModManager is not IModManager modManager) return;
            modManager.Init();

            var game = gameAssemblyContext.LoadFromAssemblyPath(Path.GetFullPath("./DummyProgram.dll"));

            game.EntryPoint!.Invoke(null, [args]);

            modManager.DeInit();
            gameAssemblyContext.Unload();
        }
    }
}
