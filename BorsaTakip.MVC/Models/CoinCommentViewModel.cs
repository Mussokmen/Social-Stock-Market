using System;
using System.Collections.Generic;

namespace BorsaTakip.MVC.Models
{
    public class CoinCommentViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string CoinId { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public int? ParentCommentId { get; set; }
        public List<CoinCommentViewModel> Replies { get; set; } = new List<CoinCommentViewModel>();
    }
}
