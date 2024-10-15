using HttpServerCore;
using WebApp.Models;

namespace WebApp.Repositories
{
    public class HttpResponseRepository
    {
        private readonly DataContext _dataContext;

        public async Task<bool> LogAsync(HttpResponse response)
        {
            throw new NotImplementedException();
        }

        //public async IAsyncEnumerable<HttpResponse> GetByFilterAsync(Func<HttpResponse, bool> filter)
        //{
        //    throw new NotImplementedException();
        //}

        //public async IAsyncEnumerable<HttpResponse> GetWithNoContentByFilterAsync(Func<HttpResponse, bool> filter)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<bool> DeleteByFilterAsync(Func<HttpResponse, bool> filter)
        {
            throw new NotImplementedException();
        }
    }
}
