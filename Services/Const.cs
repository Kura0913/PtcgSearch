namespace PtcgSearch.Services
{
    internal static class Const
    {
        /// <summary>
        /// PTCG搜尋網址
        /// </summary>
        public const string SEARCH_URL_TEMPLATE = "https://asia.pokemon-card.com/tw/card-search/list/?pageNo={0}&sortCondition=&keyword=&cardType=all&regulation=1&pokemonEnergy=&pokemonWeakness=&pokemonResistance=&pokemonMoveEnergy=&hpLowerLimit=none&hpUpperLimit=none&retreatCostLowerLimit=0&retreatCostUpperLimit=none&rarity%5B0%5D={1}&illustratorName=&expansionCodes=";
        /// <summary>
        /// PTCG卡片詳細資料網址
        /// </summary>
        public const string BASE_URL = "https://asia.pokemon-card.com";
    }
}