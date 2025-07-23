using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BorsaTakip.MVC.Models
{
    public class CoinDetailsViewModel
    {
        public JsonElement CoinDetails { get; set; }
        public List<CoinPricePoint> PriceHistory { get; set; }
        public List<CoinCommentViewModel> Comments { get; set; } = new List<CoinCommentViewModel>();
    }


}
