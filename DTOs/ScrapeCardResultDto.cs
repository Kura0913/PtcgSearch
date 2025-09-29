namespace PtcgSearch.DTOs
{
    public class ScrapeCardResultDto
{
    public int TotalRaritiesProcessed { get; set; }
    public int TotalPagesProcessed { get; set; }
    public int NewCardsAdded { get; set; }
    public int DuplicatesSkipped { get; set; }
    public required List<RaritySummary> RaritySummaries { get; set; }
    public required string Message { get; set; }
}
}