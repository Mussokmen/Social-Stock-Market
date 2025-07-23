using System.Collections.Generic;

namespace BorsaTakip.MVC.Models
{
    public class PortfolioPageViewModel
    {
        public List<UserCoinHoldingViewModel> Holdings { get; set; } = new();
        public UserCoinHoldingViewModel NewCoin { get; set; } = new();
        public decimal? TotalCurrentValue { get; set; } // Tüm portföyün güncel toplam USD değeri
    }
} 