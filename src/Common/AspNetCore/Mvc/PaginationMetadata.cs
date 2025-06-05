namespace Common.AspNetCore.Mvc;


/// <summary>
/// Pagination metadata
/// </summary>
public class PaginationMetadata : ApiMetadata
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
}