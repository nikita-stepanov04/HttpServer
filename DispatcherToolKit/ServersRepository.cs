namespace DispatcherToolKit
{
    public static class ServersRepository
    {
        private static readonly Random _random = new Random();
        private static readonly object _lock = new object();

        private static readonly List<int> _servers = new List<int>();

        public static void AddServer(int server)
        {
            lock (_lock)
            {
                _servers.Add(server);
            }
        }

        public static void RemoveServer(int server)
        {
            lock (_lock)
            {
                _servers.Remove(server);
            }
        }

        public static int? GetServer(int? requesterPort)
        {
            lock (_lock)
            {
                var availableServers = _servers
                    .Where(port => port != requesterPort)
                    .ToList();
                return availableServers.GetRandom();
            }
        }

        private static int? GetRandom(this IEnumerable<int> servers)
        {
            var serversList = servers.ToList();

            if (!serversList.Any()) return null;

            var index = _random.Next(serversList.Count);
            return serversList[index];
        }
    }
}
