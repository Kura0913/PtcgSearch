namespace PtcgSearch.DTOs
{
    public class RaritySummary
{
    public int RarityId { get; set; }
    public required string RarityName { get; set; }
    public int TotalPages { get; set; }
    public int CardsFound { get; set; }
}
}