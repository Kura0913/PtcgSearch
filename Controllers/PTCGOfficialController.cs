using Microsoft.AspNetCore.Mvc;
using PtcgSearch.Services;

namespace PtcgSearch.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PTCGOfficialController : ControllerBase
    {
        private LoadOfficialCardInfoService _cardServices;
        private readonly HttpClient _httpClient;
        public PTCGOfficialController(LoadOfficialCardInfoService cardServices, IHttpClientFactory httpClientFactory)
        {
            _cardServices = cardServices;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }
        /// <summary>
        /// 調用Service擷取所有卡片資料
        /// </summary>
        /// <returns></returns>
        [HttpPost("scrape-all-cards")]
        public async Task<IActionResult> ScrapeAllCards()
        {
            int result = await _cardServices.LoadWebCardInfoAsync(_httpClient);

            if (result == int.MinValue)
            {
                return StatusCode(500);
            }
            else if (result < 0)
            {
                return BadRequest("無法取得總頁數");
            }
            else
            {
                return Ok(new
                {
                    Message = $"成功擷取所有官方網站資料"
                });
            }
        }

        /// <summary>
        /// 調用Service清空所有卡片資料
        /// </summary>
        [HttpDelete("clear-all-cards")]
        public async Task<IActionResult> ClearAllCards()
        {
            int result = await _cardServices.RemoveAllCardWebInfo();

            return Ok(new { Message = $"已刪除 {result} 張卡片" });
        }

        [HttpPost("scrape-rarities")]
        public async Task<IActionResult> ScrapeRarities()
        {
            int result = await _cardServices.LoadAllrarities(_httpClient);

            if (result == int.MinValue)
            {
                return StatusCode(500);
            }
            else if (result < 0)
            {
                return BadRequest("無法從頁面中擷取稀有度資料");
            }
            else
            {
                return Ok(new
                {
                    TotalRarities = result
                });
            }
        }
    }
}