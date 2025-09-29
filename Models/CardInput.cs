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
        /// 1: 物品
        /// 2: 寶可夢道具
        /// 3: 支援者卡
        /// 4: 競技場
        /// 5: 特殊能量卡
        /// </summary>
        public int type { get; set; } = 0;
        /// <summary>
        /// 寶可夢屬性，若非寶可夢則預設none
        /// </summary>
        public string element { get; set; } = "none";
        /// <summary>
        /// 卡片敘述
        /// </summary>
        public string skillEffect { get; set; } = "";
        /// <summary>
        /// 是否為規則寶可夢，若非寶可夢則預設false
        /// </summary>
        public bool isRule { get; set; } = false;
        /// <summary>
        /// 寶可夢階級，若非寶可夢則為空
        /// 基礎
        /// 1階進化
        /// 2階進化
        /// </summary>
        public string evolveMarker { get; set; } = "";
        /// <summary>
        /// 寶可夢撤退費用，若非寶可夢則預設0
        /// </summary>
        public int retreatCost { get; set; } = 0;
        /// <summary>
        /// 賽制標記
        /// </summary>
        public char alpha { get; set; } = 'a';
        /// <summary>
        /// 是否為標準賽制
        /// </summary>
        public bool isRegulation { get; set; } = false;
    }
}