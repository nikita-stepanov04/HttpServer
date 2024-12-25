namespace DispatcherToolKit
{
    public class RequestForwardingException : Exception
    {
        public RequestForwardingException(string message) : base(message) { }
        public RequestForwardingException(string message, Exception inner) : base(message, inner) { }
    }

    public class DispatcherRequestException : Exception
    {
        public DispatcherRequestException(string message) : base(message) { }
        public DispatcherRequestException(string message, Exception inner) : base(message, inner) { }
    }

    public class ForwardingServerIsNotAvailableException : Exception
    {
        public ForwardingServerIsNotAvailableException(string message) : base(message) { }
        public ForwardingServerIsNotAvailableException(string message, Exception inner) : base(message, inner) { }
    }
}
