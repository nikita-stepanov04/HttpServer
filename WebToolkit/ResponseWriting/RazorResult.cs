using HttpServerCore;
using RazorLight;

namespace WebToolkit.ResponseWriting
{
    public class RazorResult<T> : IEndpointResult
    {
        private readonly string _viewName;
        private readonly T _viewModel;
        private readonly HttpResponse _httpResponse;

        private static readonly RazorLightEngine _engine = RazorHelper.Engine;

        public RazorResult(HttpResponse httpResponse, T viewModel, string viewName)
        {
            _viewName = $"Views.{viewName}";
            _viewModel = viewModel;
            _httpResponse = httpResponse;
        }

        public async Task ExecuteAsync()
        {
            var content = _httpResponse.Content;
            using var sw = new StreamWriter(content, leaveOpen: true);

            ITemplatePage template;
            var cacheResult = _engine.Handler.Cache.RetrieveTemplate(_viewName);
            if (!cacheResult.Success)
                template = await _engine.CompileTemplateAsync(_viewName);
            else
                template = cacheResult.Template.TemplatePageFactory();

            await _engine.RenderTemplateAsync(template, _viewModel, sw);
            
            await sw.FlushAsync();
            content.Position = 0;

            _httpResponse.Headers.Set("Content-Type", "text/html");
            _httpResponse.Headers.Set("Content-Length", content.Length.ToString());
            _httpResponse.StatusCode = StatusCodes.OK;            
        }
    }
}
