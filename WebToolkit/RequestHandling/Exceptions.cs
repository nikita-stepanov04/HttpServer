namespace WebToolkit.RequestHandling
{
    public class EndpointNotFoundException : Exception
    {
        public EndpointNotFoundException(string message) : base(message) {}
    } 
}
