using HttpServerCore;
using Microsoft.Extensions.Logging;
using WebToolkit.Models;

namespace DispatcherToolKit
{
    public class DispatcherRequestHandler
    {
        private readonly ILogger _logger;
        private readonly ServersRepository _servers;

        public Task GetAddress(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public Task AddAddress(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
