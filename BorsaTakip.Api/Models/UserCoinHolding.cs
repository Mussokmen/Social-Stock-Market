using System;
using System.ComponentModel.DataAnnotations;

namespace BorsaTakip.Api.Models
{
    public class UserCoinHolding
    {
        public int Id { get; set; }

        [Required]   // System.ComponentModel.DataAnnotations
        public string Username { get; set; }

        [Required]
        public string CoinName { get; set; }

        [Range(0.0001, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Range(0.0, double.MaxValue)]
        public decimal PurchasePrice { get; set; }

        public DateTime BuyDate { get; set; }
    }


}
