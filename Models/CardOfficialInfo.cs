using System;
using System.Collections.Generic;

namespace PtcgSearch.Models;

public partial class CardOfficialInfo
{
    public int CardId { get; set; }

    public string CardImage { get; set; } = null!;

    public int RarityId { get; set; }

    public bool LoadDetail { get; set; }

    public virtual Rarity Rarity { get; set; } = null!;
}
