using AmetisAPI.Domain.Bo.Gtfs;

namespace AmetisAPI.Core.Requests.Gtfs
{
    public record ImportGtfsFeedRequest(FeedSource Source) : IQuery<ValueResponse<FeedVersionInfo>>;
}
