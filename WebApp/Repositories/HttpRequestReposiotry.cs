using HttpServerCore;
using WebApp.Models;

namespace WebApp.Repositories
{
    public class HttpRequestRepository
    {
        private readonly DataContext _dataContext;

        public async Task<bool> LogAsync(HttpRequest request)
        {
            throw new NotImplementedException();
        }

        //public async IAsyncEnumerable<HttpRequest> GetByFilterAsync(Func<HttpRequest, bool> filter)
        //{
        //    throw new NotImplementedException();
        //}

        //public async IAsyncEnumerable<HttpRequest> GetWithNoContentByFilterAsync(Func<HttpRequest, bool> filter)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<bool> DeleteByFilterAsync(Func<HttpRequest, bool> filter)
        {
            throw new NotImplementedException();
        }
    }
}
