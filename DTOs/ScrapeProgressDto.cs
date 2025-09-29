namespace PtcgSearch.DTOs
{
    public class ScrapeProgressDto
{
    public int CurrentRarityId { get; set; }
    public required string CurrentRarityName { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int CardsFoundInRarity { get; set; }
}
}