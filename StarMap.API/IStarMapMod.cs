namespace StarMap.API
{
    public interface IStarMapMod : IStarMapInterface
    {
        bool ImmediateUnload { get; }
        void OnImmediatLoad();
        void OnFullyLoaded();
        void Unload();
    }
}
