using System.Runtime.Loader;

namespace StarMap.Types.Mods
{
    public interface IModManager
    {
        void Init();
        void DeInit();
    }
}
