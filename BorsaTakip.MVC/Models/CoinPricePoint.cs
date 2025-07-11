using System;
using System.Collections.Generic;
using System.Text.Json;
namespace BorsaTakip.MVC.Models
{
    public class CoinPricePoint
    {
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
    }

}

