namespace AmetisAPI.Domain.Exceptions
{
    public class DataNotFoundException : Exception
    {
        public DataNotFoundException() { }

        public DataNotFoundException(string p_Message) : base(p_Message) { }
    }
}