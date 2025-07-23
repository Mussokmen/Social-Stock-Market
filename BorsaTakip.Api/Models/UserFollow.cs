using System;

namespace BorsaTakip.Api.Models
{
    public class UserFollow
    {
        public int Id { get; set; }
        public string FollowerId { get; set; } // Takip eden kullanıcı (UserId)
        public string FollowedId { get; set; } // Takip edilen kullanıcı (UserId)
        public DateTime FollowedAt { get; set; }
    }
} 