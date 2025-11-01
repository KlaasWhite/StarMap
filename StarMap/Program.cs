using StarMap.Types.Pipes;
using System.Runtime.Loader;

namespace StarMap
{
    internal class Program
    {//test
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: StarMap <pipeName>");
                return;
            }

            var pipeName = args[0];

            MainInner(pipeName).GetAwaiter().GetResult();
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
