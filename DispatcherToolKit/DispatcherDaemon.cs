namespace DispatcherToolKit
{
    public class DispatcherDaemon
    {
        private readonly ServersRepository _serversRepository;
        private readonly HttpClient _httpClient;

        public DispatcherDaemon(ServersRepository serversRepository, HttpClient httpClient)
        {
            _serversRepository = serversRepository;
            _httpClient = httpClient;
        }

        public async Task<bool> CheckServerAvailability() => throw new NotImplementedException();
    }
}
