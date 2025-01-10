using System.Net;

namespace Akiles.Api;

public class AkilesApiException : Exception
{
    public required HttpStatusCode StatusCode { get; init; }
}
