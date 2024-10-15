using WebApp.Models;

namespace WebApp.Repositories
{
    public class UserRepository
    {
        private readonly DataContext _dataContext;

        //public async IAsyncEnumerable<User> GetUsersByFilterAsync(Func<User, bool> filter)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<AppUser> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task AddAsync(AppUser user)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteAsync(AppUser user)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateAsync(AppUser user)
        {
            throw new NotImplementedException();
        }
    }
}
