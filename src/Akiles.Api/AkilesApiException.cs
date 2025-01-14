using System.Net;

namespace Akiles.Api;

public class AkilesApiException : Exception
{
    public required Uri RequestUri { get; init; }
    public required HttpStatusCode StatusCode { get; init; }
}
