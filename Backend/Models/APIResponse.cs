using System.Net;

namespace Backend.Models;

public class APIResponse
{
    public HttpStatusCode statusCode { get; set; }
    public bool IsSuccess { get; set; } = true;
    public List<string> ErrorMasseges { get; set; } = new();
    public object? Result { get; set; }

    public static APIResponse Success(object data,
                                      HttpStatusCode code = HttpStatusCode.OK)
        => new()
        {
            IsSuccess = true,
            statusCode = code,
            Result = data
        };

    public static APIResponse Fail(string message,
                                   HttpStatusCode code = HttpStatusCode.BadRequest)
        => new()
        {
            IsSuccess = false,
            statusCode = code,
            ErrorMasseges = new() { message }
        };
}
