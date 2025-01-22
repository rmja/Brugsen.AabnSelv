using System.Net;

namespace Akiles.Api;

public class AkilesApiException(string message) : Exception(message)
{
    public required Uri RequestUri { get; init; }
    public required HttpStatusCode StatusCode { get; init; }
    public required string Type { get; init; }
    public required Dictionary<string, string> Args { get; set; }
}
