using AmetisAPI.Domain.Bo.Gtfs;

namespace AmetisAPI.Core.Requests.Gtfs
{
    public record ListFeedVersionsRequest(int Count) : IQuery<ValueResponse<IReadOnlyList<FeedVersionInfo>>>;
}
