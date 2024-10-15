using System.Collections.Concurrent;

namespace WebApp.Models
{
    public class UserStore
    {
        private readonly ConcurrentDictionary<string, User> _users = new();

        public User GetUser(string token) => throw new NotImplementedException();

        public string AddUser(User user) => throw new NotImplementedException();

        public void RemoveUser(string token) => throw new NotImplementedException();
    }
}
