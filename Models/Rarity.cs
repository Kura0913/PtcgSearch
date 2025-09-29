using System;
using System.Collections.Generic;

namespace PtcgSearch.Models;

public partial class Rarity
{
    public int RarityId { get; set; }

    public string RarityName { get; set; } = null!;

    public virtual ICollection<CardOfficialInfo> CardOfficialInfo { get; set; } = new List<CardOfficialInfo>();
}
