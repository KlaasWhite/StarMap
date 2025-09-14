namespace StarMap.Types
{
    public interface IStarMapMod
    {
        bool ImmediateUnload { get; }
        void OnImmediatLoad();
        void OnFullyLoaded();
        void Unload();
    }
}
