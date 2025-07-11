using System;
using System.ComponentModel.DataAnnotations;

namespace BorsaTakip.MVC.Models
{
    public class CommentReplyViewModel
    {
        public int Id { get; set; }

        [Required]
        public int CoinCommentId { get; set; }  // Yanıt verilen yorumun Id'si

        [Required]
        public string Username { get; set; }

        [Required]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
