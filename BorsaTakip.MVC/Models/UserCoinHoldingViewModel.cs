using System;
using System.ComponentModel.DataAnnotations;

namespace BorsaTakip.MVC.Models
{
    public class UserCoinHoldingViewModel
    {
        public int Id { get; set; }

        [Required]
        public string CoinName { get; set; }

        [Required]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Adet pozitif olmalı.")]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0.0, double.MaxValue, ErrorMessage = "Fiyat negatif olamaz.")]
        public decimal PurchasePrice { get; set; }

        [Display(Name = "Alış Tarihi")]
        public DateTime BuyDate { get; set; } = DateTime.UtcNow;

        public decimal? CurrentPrice { get; set; } // Güncel fiyat (USD)
        public decimal? CurrentTotalValue { get; set; } // Güncel toplam değer (USD)

        public string Username { get; set; } // Formda gösterilmeyecek
    }
} 