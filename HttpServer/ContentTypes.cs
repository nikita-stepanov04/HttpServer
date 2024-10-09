namespace HttpServerCore
{
    public class ContentTypes
    {
        public static string Parse(string extension) =>
            extension switch
            {
                "txt"  => "text/plain",
                "html" => "text/html",
                "css"  => "text/css",
                "jpg"  => "image/jpeg",
                "png"  => "image/png",
                "json" => "application/json",
                "js"   => "application/javascript",
                _       => "application/octet-stream"
            };
    }
}
