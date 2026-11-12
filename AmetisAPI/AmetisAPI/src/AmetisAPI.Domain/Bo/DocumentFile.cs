namespace AmetisAPI.Domain.Bo
{
    public record DocumentFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public Stream StreamContent { get; set; }
        public byte[] Content { get; set; }
        public Uri Uri { get; set; }
    }
}