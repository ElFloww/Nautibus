using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Entities
{
    public class ImportRejectionEntity : IEntity
    {
        public long Id { get; set; }
        public int FeedVersionId { get; set; }
        public string FileName { get; set; }
        public int LineNumber { get; set; }
        public string Reason { get; set; }
    }
}
