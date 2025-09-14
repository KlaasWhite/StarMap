
using StarMap.Types;
using TestMod.Dependency;

namespace TestMod2
{
    public class TestMod2 : IStarMapMod
    {
        public bool ImmediateUnload => false;

        //private static Harmony? _harmony = new Harmony("DoSomething");

        public void OnFullyLoaded()
        {
            //var myClass = new DependencyClass();
            //var original = typeof(DummyProgram.Screens.GameScreen).GetMethod("DoSomething");
            //var prefix = typeof(Patcher).GetMethod("DoSomething");

            //_harmony?.Patch(original, prefix: new HarmonyMethod(prefix));
        }

        public void OnImmediatLoad()
        {
        }

        public void Unload()
        {
            //_harmony.UnpatchAll(_harmony.Id);
            //Patcher.Unload();
        }
    }
}
