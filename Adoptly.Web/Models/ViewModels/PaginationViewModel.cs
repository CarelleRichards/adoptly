namespace Adoptly.Web.Models;

public class PaginationViewModel
{
    public required int TotalResults { get; set; }
    public required int TotalPages { get; set; }
    public required int CurrentPage { get; set; }
    public required string Url { get; set; }
    public static int StartPage { get; } = 1;
}