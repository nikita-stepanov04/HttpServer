using RazorLight;

namespace WebToolkit.ResponseWriting
{
    public static class RazorHelper
    {
        public static RazorLightEngine? _engine;

        public static void ConfigureEngine(Type assembly)
        {
            _engine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(assembly)
                .SetOperatingAssembly(assembly.Assembly)
                .EnableDebugMode()
                .UseMemoryCachingProvider()
                .Build();
        }

        public static RazorLightEngine Engine => _engine 
            ?? throw new NullReferenceException("Engine is not yet configured");
    }
}
