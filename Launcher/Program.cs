using System.Diagnostics;

namespace Launcher
{
    internal class Program
    {
        const string ProjectName = "Launcher";
        const string WebAppProjectName = "WebApp";
        const string DispatcherProjectName = "Dispatcher";

        const int DispatcherPort = 8080;
        const int WebAppStartPort = 8081;
        const int WebAppInstancesCount = 2;

        static void Main(string[] args)
        {
            string baseDirectory = AppContext.BaseDirectory;

            string webAppProjectPath = $"{baseDirectory.Replace(ProjectName, WebAppProjectName)}{WebAppProjectName}.exe";
            string dispatcherProjectPath = $"{baseDirectory.Replace(ProjectName, DispatcherProjectName)}{DispatcherProjectName}.exe";

            StartInstance(dispatcherProjectPath, DispatcherPort.ToString());
            Task.Delay(2000).Wait();

            for (int i = 0; i < WebAppInstancesCount; i++)
            {
                StartInstance(webAppProjectPath, $"{WebAppStartPort + i} {DispatcherPort}");
            }
        }

        private static void StartInstance(string webAppPath, string arguments)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = webAppPath,
                Arguments = arguments,
                UseShellExecute = true,
                CreateNoWindow = false 
            }; 
            Process.Start(processStartInfo); }
        }
}
