using System;
namespace BorsaTakip.Api.Models
{
    public class CommentLike
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties (isteğe bağlı)
        public CoinComment Comment { get; set; }
        public ApplicationUser User { get; set; }
    }

}

