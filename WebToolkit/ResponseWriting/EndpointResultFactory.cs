using HttpServerCore;

namespace WebToolkit.ResponseWriting
{
    public static class EndpointResultFactory
    {
        public static StaticResult StaticResult(this HttpResponse response, string filePath)
        {
            return new StaticResult(response, filePath);
        }
        
        public static JsonResult<T> JsonResult<T>(this HttpResponse response, T entity)
        {
            return new JsonResult<T>(response, entity);
        }
        
        public static HttpStatusResult HttpStatusResult(this HttpResponse response, StatusCodes statusCode)
        {
            return new HttpStatusResult(response, statusCode);
        }
        
        public static RazorResult<T> RazorResult<T>(this HttpResponse response, T viewModel, string viewName)
        {
            return new RazorResult<T>(response, viewModel, viewName);
        }
    }
}
