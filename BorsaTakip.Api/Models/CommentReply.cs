using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BorsaTakip.Api.Models
{
    // CommentReply.cs (Model)
    public class CommentReply
    {
        public int Id { get; set; }
        public int CoinCommentId { get; set; } // Yorum ID
        public string Username { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property, required olmamalı
        public CoinComment CoinComment { get; set; }
    }

}
