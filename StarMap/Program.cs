using StarMap.Types;
using StarMap.Types.Pipes;
using StarMap.Types.Proto.IPC;
using System.Runtime.Loader;

namespace StarMap
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Running Starmap in dumb mode!");
                DumbModeInner();
                return;
            }

            Console.WriteLine("Running Starmap normally.");

            var pipeName = args[0];
            Console.WriteLine($"Connection to pipe: {pipeName}");

            MainInner(pipeName).GetAwaiter().GetResult();
        }

        static void DumbModeInner()
        {
            var gameConfig = new LoaderConfig();

            if (!gameConfig.TryLoadConfig())
            {
                return;
            }

            AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath("./0Harmony.dll"));

            var gameAssemblyContext = new GameAssemblyLoadContext(gameConfig.GameLocation);
            var dumbFacade = new DumbGameFacade();
            var gameSurveyer = new GameSurveyer(dumbFacade, gameAssemblyContext, gameConfig.GameLocation);
            if (!gameSurveyer.TryLoadModManagerAndGame(out _))
            {
                Console.WriteLine("Unable to load mod manager and game in dumb mode.");
                return;
            }

            gameSurveyer.RunGame();
        }

        static async Task MainInner(string pipeName)
        {
            AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath("./0Harmony.dll"));

            var pipeClient = new PipeClient(pipeName);
            var facade = new GameFacade(pipeClient);

            var gameLocation = await facade.Connect();
            var gameAssemblyContext = new GameAssemblyLoadContext(Path.GetFullPath(gameLocation));
            var gameSurveyer = new GameSurveyer(facade, gameAssemblyContext, gameLocation);
            if (!gameSurveyer.TryLoadModManagerAndGame(out _)) return;

            gameSurveyer.RunGame();

            await facade.DisposeAsync();
        }
    }
}
