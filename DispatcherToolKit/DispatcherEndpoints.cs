using HttpServerCore;
using WebToolkit;
using WebToolkit.ResponseWriting;

namespace DispatcherToolKit
{
    public static class DispatcherEndpoints
    {
        public static async Task GetAddress(HttpContext context)
        {
            int? requestPort = await context.Request.ReadJsonAsync<int?>();
            int? port = ServersRepository.GetServer(requestPort);

            await context.Response.JsonResult(port).ExecuteAsync();
        }

        public static async Task RegisterServer(HttpContext context)
        {
            int? requestPort = await context.Request.ReadJsonAsync<int?>();
            if (!requestPort.HasValue)
            {
                await context.Response.HttpStatusResult(StatusCodes.BadRequest).ExecuteAsync();
                return;
            }
            ServersRepository.AddServer(requestPort.Value);
            await context.Response.HttpStatusResult(StatusCodes.NoContent).ExecuteAsync();
        }

        public static async Task UnregisterServer(HttpContext context)
        {
            int? requestPort = await context.Request.ReadJsonAsync<int?>();
            if (!requestPort.HasValue)
            {
                await context.Response.HttpStatusResult(StatusCodes.BadRequest).ExecuteAsync();
                return;
            }
            ServersRepository.RemoveServer(requestPort.Value);
            await context.Response.HttpStatusResult(StatusCodes.NoContent).ExecuteAsync();
        }
    }
}
