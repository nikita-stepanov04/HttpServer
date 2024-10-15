using WebApp.Models;

namespace WebApp.Repositories
{
    public class ServerLogRepository
    {
        private readonly DataContext _dataContext;

        //public async IAsyncEnumerable<ServerLogRepository> GetByFilterAsync(Func<ServerLog, bool> filter)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<bool> DeleteByFilterAsync(Func<ServerLog, bool> filter)
        {
            throw new NotImplementedException();
        }
    }
}
