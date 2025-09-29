using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace PtcgSearch
{
    public class CardInput
    {
        /// <summary>
        /// 卡片名稱
        /// </summary>
        public string name { get; set; } = "";
        /// <summary>
        /// 卡片id
        /// </summary>
        public string id { get; set; } = "";
        /// <summary>
        /// 卡片種類: 
        /// 0: 寶可夢
        /// 1: 訓練家
        /// 2: 競技場
        /// 3: 特殊能量
        /// </summary>
        public int type { get; set; } = 0;
        /// <summary>
        /// 寶可夢屬性，若非寶可夢則預設none
        /// </summary>
        public string element { get; set; } = "none";
        /// <summary>
        /// 卡片敘述
        /// </summary>
        public string description { get; set; } = "";
        /// <summary>
        /// 是否為規則寶可夢，若非寶可夢則預設false
        /// </summary>
        public bool isRule { get; set; } = false;
        /// <summary>
        /// 寶可夢階級，若非寶可夢則預設0
        /// 0: 基礎寶可夢
        /// 1: 1階進化寶可夢
        /// 2: 2階進化寶可夢
        /// </summary>
        public int levelEvolution { get; set; } = 0;
        /// <summary>
        /// 寶可夢撤退費用，若非寶可夢則預設0
        /// </summary>
        public int retreatCost { get; set; } = 0;
    }
}