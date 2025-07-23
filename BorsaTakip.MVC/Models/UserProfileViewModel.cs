using System;
using System.Collections.Generic;

namespace BorsaTakip.MVC.Models
{
    public class UserProfileViewModel
    {
        public string Username { get; set; }

        public List<CoinCommentViewModel> Comments { get; set; }
        public string Bio { get; set; }
        public string PhotoUrl { get; set; }
        public List<BadgeViewModel> Badges { get; set; } = new List<BadgeViewModel>();
        public int TotalPoints { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public bool IsOwnProfile { get; set; }


    }

    public class BadgeViewModel
    {
        public string BadgeType { get; set; }
        public DateTime EarnedAt { get; set; }
    }
}
