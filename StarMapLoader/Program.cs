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
        {//Test workflow
            var config = new LoaderConfig();
            var modRepository = new ModRepository(config.ModPath);

            var shouldReload = true;

            var pipeName = Debugger.IsAttached ? "starmap_pipe" : $"starmap_pipe_{Guid.NewGuid()}";
            using var pipeServer = new PipeServer(pipeName);
            using var facade = new LoaderFacade(pipeServer, config, modRepository);

            while (shouldReload)
            {
                CancellationTokenSource stopGameCancelationTokenSource = new();

                var gameSupervisor = new GameProcessSupervisor(config.GamePath, facade, pipeServer);

                await await gameSupervisor.TryStartGameAsync(stopGameCancelationTokenSource.Token);

                shouldReload = modRepository.HasChanges;
                if (shouldReload)
                {
                    modRepository.ApplyModUpdates();
                }
            }
        }
    }
}
