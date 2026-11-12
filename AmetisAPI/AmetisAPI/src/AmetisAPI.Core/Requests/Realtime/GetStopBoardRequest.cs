using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Core.Requests.Realtime
{
    public record GetStopBoardRequest(string StopId, int Limit = 25) : IQuery<ValueResponse<StopBoard>>;
}
