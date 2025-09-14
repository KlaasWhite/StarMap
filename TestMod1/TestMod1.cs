using StarMap.Types;
using TestMod.Dependency;

namespace TestMod1
{
    public class TestMod1 : IStarMapMod
    {
        public bool ImmediateUnload => false;

        public void OnFullyLoaded()
        {
            var myClass = new DependencyClass();
            Patcher.Patch();
        }

        public void OnImmediatLoad()
        {
        }

        public void Unload()
        {
            Patcher.Unload();
        }
    }
}
