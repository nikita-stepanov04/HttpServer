using RazorLight;
using System.Reflection;

namespace WebToolkit.ResponseWriting
{
    public static class RazorHelper
    {
        private static RazorLightEngine? _engine;

        public static Assembly? Assembly { get; private set; }

        public static void ConfigureEngine(Type assembly)
        {
            Assembly = assembly.Assembly;
            _engine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(assembly)
                .SetOperatingAssembly(assembly.Assembly)
                .UseMemoryCachingProvider()
                .Build();
        }

        public static async Task PrecompileViews()
        {
            if (Assembly == null) throw new ArgumentNullException(nameof(Assembly));

            var tasks = new List<Task<ITemplatePage>>();
            foreach (var resource in Assembly.GetManifestResourceNames())
            {
                string viewName = "Views" + resource.Split("Views")[1];
                tasks.Add(_engine!.CompileTemplateAsync(viewName));
            }
            await Task.WhenAll(tasks);
        }

        public static RazorLightEngine Engine => _engine
            ?? throw new NullReferenceException("Engine is not yet configured");
    }
}
