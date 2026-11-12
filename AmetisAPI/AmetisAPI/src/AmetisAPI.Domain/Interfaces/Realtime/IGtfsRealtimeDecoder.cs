using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Domain.Interfaces.Realtime
{
    public interface IGtfsRealtimeDecoder
    {
        /// <summary>Décode un FeedMessage protobuf. Lève une exception si le contenu n'est pas décodable.</summary>
        RealtimeFeed Decode(byte[] p_Content);
    }
}
