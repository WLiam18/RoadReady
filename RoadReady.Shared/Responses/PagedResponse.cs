namespace RoadReady.Shared.Responses;

public class PagedResponse<T>
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = "Request successful.";
    public List<T> Data { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    public static PagedResponse<T> Create(List<T> data, int page, int pageSize, int totalCount)
        => new() { Data = data, Page = page, PageSize = pageSize, TotalCount = totalCount };
}
