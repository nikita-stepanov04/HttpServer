using HttpServerCore;
using Microsoft.Extensions.Logging;

namespace DispatcherToolKit
{
    public class DispatcherRequestHandler
    {
        private readonly ILogger _logger;
        private readonly ServersRepository _servers;

        public Task GetAddress(HttpRequest request, HttpResponse response)
        {
            throw new NotImplementedException();
        }

        public Task AddAddress(HttpRequest request, HttpResponse response)
        {
            throw new NotImplementedException();
        }
    }
}
