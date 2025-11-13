using System.Reflection;

namespace KSA
{
    public class Mod
    {
        public required string DirectoryPath { get; init; }
        public required AssemblyName Assembly { get; init; }
        public required string Name { get; init; }

        public Mod() { }
        public void PrepareSystems()
        {
        }

        public void DoSomething()
        {
            Console.WriteLine($"Mod {Name} is doing something!");
        }
    }
}
