namespace HttpServerCore
{
    public class ContentTypes
    {
        public static string Parse(string extension) =>
            extension switch
            {
                "txt" => "text/plain",
                "html" => "text/html",
                "css" => "text/css",
                "jpg" => "image/jpeg",
                "jpeg" => "image/jpeg",
                "png" => "image/png",
                "gif" => "image/gif",
                "bmp" => "image/bmp",
                "ico" => "image/x-icon",
                "svg" => "image/svg+xml",
                "json" => "application/json",
                "js" => "application/javascript",
                "xml" => "application/xml",
                _ => "application/octet-stream"
            };
    }
}
