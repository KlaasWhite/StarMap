using StarMap.Types.Pipes;
using System.Diagnostics;

namespace StarMapLoader
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MainInner().GetAwaiter().GetResult();
        }

        static async Task MainInner()
        {
            var modRepository = new ModRepository(Path.Combine(Path.GetFullPath("./"), "mods"));
            var shouldReload = true;

            while (shouldReload)
            {
                var config = new LoaderConfig();

                var pipeName = Debugger.IsAttached ? "starmap_pipe" : $"starmap_pipe_{Guid.NewGuid()}";
                using var pipeServer = new PipeServer(pipeName);
                

                var facade = new LoaderFacade(pipeServer, config, modRepository);

                var gameSupervisor = new GameProcessSupervisor("StarMap.exe", facade, pipeServer);

                await await gameSupervisor.TryStartGameAsync();

                shouldReload = modRepository.HasChanges;
                if (shouldReload)
                {
                    modRepository.ApplyModUpdates();
                }
            }
        }
    }
}
