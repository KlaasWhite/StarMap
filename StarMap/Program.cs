using StarMap.Types.Pipes;
using System.Runtime.Loader;

namespace StarMap
{
    internal class Program
    {
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

        static async Task MainInner(string pipename)
        {
            AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath("./0Harmony.dll"));

            var pipeClient = new PipeClient(pipename);
            var facade = new GameFacade(pipeClient);
            
            var gameLocation = await facade.Connect();
            var gameAssemblyContext = new GameAssemblyLoadContext(Path.GetFullPath(gameLocation));
            var gameSurveyer = new GameSurveyer(facade, gameAssemblyContext, gameLocation);
            gameSurveyer.LoadModManagerAndGame();

            await gameSurveyer.RunGame();
        }
    }
}
