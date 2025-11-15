namespace StarMap.API
{
    public interface IStarMapOnUi : IStarMapInterface
    {
        void OnBeforeUi(double dt);
        void OnAfterUi(double dt);
    }
}
