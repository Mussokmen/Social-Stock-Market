using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BorsaTakip.Api.Models
{
    public class CoinComment
    {
        public int Id { get; set; }
        public string CoinId { get; set; }
        public string Username { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int LikeCount { get; set; } = 0;
        


    }
}

