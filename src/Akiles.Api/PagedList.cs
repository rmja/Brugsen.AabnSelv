namespace Akiles.Api;

public class PagedList<T>
{
    public required List<T> Data { get; set; }
    public bool HasNext { get; set; }
    public string? CursorNext { get; set; }
}
