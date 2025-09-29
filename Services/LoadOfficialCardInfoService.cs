

using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using PtcgSearch.Models;
using PtcgSearch.DTOs;
namespace PtcgSearch.Services
{
    public class LoadOfficialCardInfoService
    {
        private PtcgCardContext _dbcontext;
        public LoadOfficialCardInfoService(PtcgCardContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        /// <summary>
        /// 擷取所有卡片資料並儲存至資料庫
        /// </summary>
        /// <param name="_httpClient"></param>
        /// <returns></returns>
        public async Task<int> LoadWebCardInfoAsync(HttpClient _httpClient)
        {
            try
            {
                // 1. 從資料庫取得所有稀有度
                var rarities = await _dbcontext.Rarity.OrderBy(r => r.RarityId).ToListAsync();

                if (!rarities.Any())
                {
                    return -1;
                }

                var allCards = new List<CardOfficialInfo>();
                var duplicateCount = 0;
                var totalPagesProcessed = 0;
                var raritySummaries = new List<RaritySummary>();

                // 2. 外層迴圈：遍歷每個稀有度
                foreach (var rarity in rarities)
                {
                    Console.WriteLine($"開始處理稀有度: {rarity.RarityName} (ID: {rarity.RarityId})");

                    var cardsInRarity = new List<CardOfficialInfo>();
                    
                    // 2.1 取得該稀有度的第一頁以獲取總頁數
                    var firstPageUrl = string.Format(Const.SEARCH_URL_TEMPLATE, 1, rarity.RarityId);
                    var firstPageHtml = await _httpClient.GetStringAsync(firstPageUrl);
                    
                    var totalPages = GetTotalPages(firstPageHtml);
                    
                    if (totalPages == 0)
                    {
                        Console.WriteLine($"稀有度 {rarity.RarityName} 沒有卡片或無法取得頁數");
                        raritySummaries.Add(new RaritySummary
                        {
                            RarityId = rarity.RarityId,
                            RarityName = rarity.RarityName,
                            TotalPages = 0,
                            CardsFound = 0
                        });
                        continue;
                    }

                    Console.WriteLine($"稀有度 {rarity.RarityName} 共有 {totalPages} 頁");

                    // 2.2 內層迴圈：遍歷該稀有度的所有頁面
                    for (int pageNo = 1; pageNo <= totalPages; pageNo++)
                    {
                        var pageUrl = string.Format(Const.SEARCH_URL_TEMPLATE, pageNo, rarity.RarityId);
                        var pageHtml = await _httpClient.GetStringAsync(pageUrl);
                        
                        var cardsOnPage = ExtractCardsFromPage(pageHtml, rarity.RarityId);
                        cardsInRarity.AddRange(cardsOnPage);
                        
                        Console.WriteLine($"  頁面 {pageNo}/{totalPages}: 找到 {cardsOnPage.Count} 張卡片");

                        // 避免過度請求
                        await Task.Delay(100);
                    }

                    totalPagesProcessed += totalPages;

                    // 2.3 檢查重複並儲存該稀有度的卡片
                    var newCardsInRarity = 0;
                    var duplicatesInRarity = 0;
                    var cardsToSave = new List<CardOfficialInfo>();

                    foreach (var card in cardsInRarity)
                    {
                        var exists = await _dbcontext.CardOfficialInfo.AnyAsync(c => c.CardId == card.CardId);
                        if (!exists)
                        {
                            // 檢查是否在當前批次中重複
                            if (!cardsToSave.Any(c => c.CardId == card.CardId))
                            {
                                cardsToSave.Add(card);
                                newCardsInRarity++;
                            }
                            else
                            {
                                duplicatesInRarity++;
                            }
                        }
                        else
                        {
                            duplicatesInRarity++;
                        }
                    }

                    // 2.4 立即儲存該稀有度的卡片到資料庫
                    if (cardsToSave.Any())
                    {
                        await _dbcontext.CardOfficialInfo.AddRangeAsync(cardsToSave);
                        await _dbcontext.SaveChangesAsync();
                        Console.WriteLine($"  ✓ 已儲存 {cardsToSave.Count} 張卡片到資料庫");
                    }

                    allCards.AddRange(cardsToSave);
                    duplicateCount += duplicatesInRarity;

                    raritySummaries.Add(new RaritySummary
                    {
                        RarityId = rarity.RarityId,
                        RarityName = rarity.RarityName,
                        TotalPages = totalPages,
                        CardsFound = cardsInRarity.Count
                    });

                    Console.WriteLine($"稀有度 {rarity.RarityName} 完成: 總共 {cardsInRarity.Count} 張卡片，新增 {newCardsInRarity} 張，重複 {duplicatesInRarity} 張\n");
                }
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"發生例外:{e}");
                return int.MinValue;
            }
        }
        /// <summary>
        /// 清除所有卡片資料
        /// </summary>
        /// <returns></returns>
        public async Task<int> RemoveAllCardWebInfo()
        {
            var allCards = await _dbcontext.CardOfficialInfo.ToListAsync();
            _dbcontext.CardOfficialInfo.RemoveRange(allCards);
            await _dbcontext.SaveChangesAsync();

            return allCards.Count;
        }
        /// <summary>
        /// 取得Rarity代號
        /// </summary>
        /// <param name="_httpClient"></param>
        /// <returns></returns>
        public async Task<int> LoadAllrarities(HttpClient _httpClient)
        {
            try
            {
                // 1. 取得搜尋頁面的 HTML
                var firstPageUrl = string.Format(Const.SEARCH_URL_TEMPLATE, 1);
                var html = await _httpClient.GetStringAsync(firstPageUrl);

                // 2. 解析 HTML 並提取稀有度資訊
                var rarities = ExtractRaritiesFromHtml(html);

                if (!rarities.Any())
                {
                    return -1;
                }

                // 3. 檢查並儲存至資料庫
                var newRarities = new List<Rarity>();
                var updatedRarities = new List<Rarity>();

                foreach (var rarity in rarities)
                {
                    var existingRarity = await _dbcontext.Rarity
                        .FirstOrDefaultAsync(r => r.RarityId == rarity.RarityId);

                    if (existingRarity == null)
                    {
                        newRarities.Add(rarity);
                    }
                    else if (existingRarity.RarityName != rarity.RarityName)
                    {
                        existingRarity.RarityName = rarity.RarityName;
                        updatedRarities.Add(existingRarity);
                    }
                }

                // 4. 儲存變更
                if (newRarities.Any())
                {
                    await _dbcontext.Rarity.AddRangeAsync(newRarities);
                }

                await _dbcontext.SaveChangesAsync();

                return rarities.Count;
            }
            catch (Exception)
            {
                return int.MinValue;
            }
        }



        /// <summary>
        /// 從 HTML 中提取總頁數
        /// </summary>
        private int GetTotalPages(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // 尋找 resultTotalPages
            var totalPagesNode = doc.DocumentNode.SelectSingleNode("//p[@class='resultTotalPages']");

            if (totalPagesNode != null)
            {
                var match = Regex.Match(totalPagesNode.InnerText, @"(\d+)\s*頁");
                if (match.Success)
                {
                    return int.Parse(match.Groups[1].Value);
                }
            }

            return 0;
        }
        /// <summary>
        /// 從頁面 HTML 中提取所有卡片資訊
        /// </summary>
        private List<CardOfficialInfo> ExtractCardsFromPage(string html, int rarityId)
    {
        var cards = new List<CardOfficialInfo>();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // 選取所有卡片元素
        var cardNodes = doc.DocumentNode.SelectNodes("//li[@class='card']");

        if (cardNodes != null)
        {
            foreach (var cardNode in cardNodes)
            {
                try
                {
                    // 提取卡片編號
                    var linkNode = cardNode.SelectSingleNode(".//a[@href]");
                    var href = linkNode?.GetAttributeValue("href", "");
                    
#pragma warning disable CS8604 // 可能有 Null 參考引數。
                    var cardIdMatch = Regex.Match(href, @"/card-search/detail/(\d+)/");
#pragma warning restore CS8604 // 可能有 Null 參考引數。
                    if (!cardIdMatch.Success) continue;
                    
                    var cardId = int.Parse(cardIdMatch.Groups[1].Value);

                    // 提取圖片 URL
                    var imgNode = cardNode.SelectSingleNode(".//img[@data-original]");
                    var cardImage = imgNode?.GetAttributeValue("data-original", "");

                    if (!string.IsNullOrEmpty(cardImage))
                    {
                        cards.Add(new CardOfficialInfo
                        {
                            CardId = cardId,
                            CardImage = cardImage,
                            RarityId = rarityId,
                            LoadDetail = false
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing card: {ex.Message}");
                }
            }
        }

        return cards;
    }
        
        /// <summary>
        /// 從 HTML 中提取稀有度資訊
        /// </summary>
        private List<Rarity> ExtractRaritiesFromHtml(string html)
        {
            var rarities = new List<Rarity>();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // 尋找 rarities div
            var raritiesDiv = doc.DocumentNode.SelectSingleNode("//div[@class='rarities']");

            if (raritiesDiv != null)
            {
                // 選取所有 rarityOption
                var rarityNodes = raritiesDiv.SelectNodes(".//div[contains(@class, 'rarityOption')]");

                if (rarityNodes != null)
                {
                    foreach (var rarityNode in rarityNodes)
                    {
                        try
                        {
                            // 提取 input 元素的 value 屬性 (RarityId)
                            var inputNode = rarityNode.SelectSingleNode(".//input[@name='rarity[]']");
                            var rarityIdStr = inputNode?.GetAttributeValue("value", "");

                            // 提取 label 元素的文字內容 (RarityName)
                            var labelNode = rarityNode.SelectSingleNode(".//label");
                            var rarityName = labelNode?.InnerText?.Trim();

                            if (!string.IsNullOrEmpty(rarityIdStr) && 
                                !string.IsNullOrEmpty(rarityName) && 
                                int.TryParse(rarityIdStr, out int rarityId))
                            {
                                rarities.Add(new Rarity
                                {
                                    RarityId = rarityId,
                                    RarityName = rarityName
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing rarity: {ex.Message}");
                        }
                    }
                }
            }
            return rarities;
        }
    }
}