namespace StarMap.API
{
    public interface IStarMapMod : IStarMapInterface
    {
        bool ImmediateUnload { get; }
        void OnImmediateLoad();
        void OnFullyLoaded();
        void Unload();
    }
}
