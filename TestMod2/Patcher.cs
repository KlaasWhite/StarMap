using HarmonyLib;
using System.Reflection;
using TestMod.Dependency;

namespace TestMod2
{
    [HarmonyPatch]
    internal static class Patcher
    {
        private static Harmony? _harmony = new Harmony("TestMod2");

        public static void Patch()
        {
            _harmony?.PatchAll();
        }

        public static void Unload()
        {
            _harmony?.UnpatchAll(_harmony.Id);
            _harmony = null;
        }

        [HarmonyPatch(typeof(DummyProgram.Screens.GameScreen), nameof(DummyProgram.Screens.GameScreen.DoSomething))]
        [HarmonyPrefix]
        public static void DoSomething(this DummyProgram.Screens.GameScreen __instance)
        {
            var modAssembly = Assembly.GetExecutingAssembly();
            var depAssembly = typeof(DependencyClass).Assembly;

            Console.WriteLine($"Hello from {modAssembly.GetName().Name}:{modAssembly.GetName().Version} with dependency {depAssembly.GetName().Name}:{depAssembly.GetName().Version}");
        }
    }
}
