using DispatcherToolKit.Handlers;
using HttpServerCore.Request;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebToolkit;
using WebToolkit.RequestHandling;
using HttpClient = HttpServerCore.Server.HttpClient;

namespace DispatcherToolKit.Middleware
{
    public class RequestForwardingMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, Func<Task> next)
        {
            bool? forward = context.Request.QueryParams.GetBool("forward");
            if (forward.HasValue && forward.Value)
            {
                context.Request.QueryParams["forward"] = "invoked";
                var client = new HttpClient(context.LoggerFactory);

                var logger = context.LoggerFactory.CreateLogger<RequestForwardingMiddleware>();

                int serverPort = await RequestDispatcher(context, logger);
                await ForwardRequest(context, logger, serverPort);
            }
            else
                await next();
        }

        private static async Task<int> RequestDispatcher(HttpContext context, ILogger logger)
        {
            var client = new HttpClient(context.LoggerFactory);
            var request = new HttpRequest()
            {
                Method = "GET",
                Uri = "/get"
            };
            await request.WriteJsonAsync(context.Request.ServerPort);
            var response = new HttpResponse();

            logger = context.LoggerFactory.CreateLogger<RequestForwardingMiddleware>();
            logger.LogInformation("Requesting dispatcher");

            try
            {
                await client.SendAsync(request, response, DispatcherConnection.DispatcherPort);
            }
            catch (Exception e)
            {
                throw new DispatcherRequestException("Failed to request the dispatcher", e);
            }

            var reader = new StreamReader(response.Content);
            string responseJson = reader.ReadToEnd();
            int? forwardPort = JsonSerializer.Deserialize<int?>(responseJson);

            if (!forwardPort.HasValue) throw new ForwardingServerIsNotAvailableException("Dispatcher returned no server port");

            logger.LogInformation("Successfully received server address from dispatcher");
            return forwardPort.Value;
        }

        private static async Task ForwardRequest(HttpContext context, ILogger logger, int forwardPort)
        {
            logger.LogInformation("Forwarding request to server: {p}", forwardPort);

            var client = new HttpClient(context.LoggerFactory);
            context.Request.ServerPort = forwardPort;

            try
            {
                await client.SendAsync(context.Request, context.Response, forwardPort);
            }
            catch (Exception e)
            {
                throw new RequestForwardingException($"Failed to forward request to server {forwardPort}", e);
            }

            if ((int)context.Response.StatusCode < 400)
                logger.LogInformation("Successfully invoked request forwarding with status code: {s}", context.Response.StatusCode);
            else
                logger.LogError("Request forwarding invoked with status code: {s}", context.Response.StatusCode);
        }
    }
}
