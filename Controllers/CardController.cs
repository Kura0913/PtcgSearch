


using Microsoft.AspNetCore.Mvc;
using PtcgSearch.Services;

namespace PtcgSearch.Controllers
{
    public class CardController : Controller
    {
        private CardServices _cardServices;

        public CardController(CardServices cardServices)
        {
            _cardServices = cardServices;
        }

        [HttpPost("basic")]
        public async Task<IActionResult> CreateCardAsync([FromBody] CardInput card)
        {
            int result = await _cardServices.CreateCardAsync(card);

            return Ok(result);
        }
    }
}