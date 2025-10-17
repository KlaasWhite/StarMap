using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace StarMap.Types.Mods
{
    public interface IGameFacade
    {
        Task<Any> RequestData(IMessage request);
    }
}
