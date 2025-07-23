using System;

namespace BorsaTakip.Api.Models
{
    public class UserBadge
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string BadgeType { get; set; } // "PopulerYorumcu", "SurekliYorumcu", ...
        public DateTime EarnedAt { get; set; }
    }
} 