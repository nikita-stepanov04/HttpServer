using WebApp.Models;

namespace WebApp.Repositories
{
    public class UserRepository
    {
        private readonly DataContext _dataContext;

        public async IAsyncEnumerable<User> GetUsersByFilterAsync(Func<User, bool> filter)
        {
            throw new NotImplementedException();
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<User> AddAsync(User user)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteAsync(User user)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateAsync(User user)
        {
            throw new NotImplementedException();
        }
    }
}
