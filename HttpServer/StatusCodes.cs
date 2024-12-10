namespace HttpServerCore
{
    public enum StatusCodes
    {
        OK = 200,
        NoContent = 204,

        BadRequest = 400,
        NotFound = 404,
        MethodNotAllowed = 405,

        InternalServerError = 500,
        HttpVersionNotSupported = 505,
    }

}
