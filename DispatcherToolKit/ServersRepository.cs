using System.Text.Json;

namespace DispatcherToolKit
{
    public static class ServersRepository
    {
        private static Random _random = new Random();

        private static readonly List<int> _servers = new();

        public static void AddServer(int server) => _servers.Add(server);

        public static void RemoveServer(int server) => _servers.Remove(server);

        public static int? GetServer(int? requesterPort) => _servers
            .Where(port => port != requesterPort)
            .GetRandom();

        private static int? GetRandom(this IEnumerable<int> servers)
        {
            var serversList = servers.ToList();

            if (!serversList.Any()) return null;

            var index = _random.Next(serversList.Count);
            return serversList[index];
        }
    }
}
