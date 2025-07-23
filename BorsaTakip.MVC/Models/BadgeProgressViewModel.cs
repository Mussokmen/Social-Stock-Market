using System;
using System.Collections.Generic;

namespace BorsaTakip.MVC.Models
{
    public class BadgeProgressViewModel
    {
        public string BadgeType { get; set; }
        public string BadgeName { get; set; }
        public string Description { get; set; }
        public int CurrentValue { get; set; }
        public int TargetValue { get; set; }
        public bool Earned { get; set; }
        public int Remaining => Math.Max(TargetValue - CurrentValue, 0);
    }

    public class BadgePointsViewModel
    {
        public int TotalPoints { get; set; }
        public int YorumPuan { get; set; }
        public int BegeniPuan { get; set; }
        public int YanitPuan { get; set; }
        public int RozetPuan { get; set; }
        public int GunlukBonus { get; set; }
        public List<BadgeProgressViewModel> Badges { get; set; } = new List<BadgeProgressViewModel>();
        public List<LeaderboardEntryViewModel> Leaderboard { get; set; } = new List<LeaderboardEntryViewModel>();
    }

    public class LeaderboardEntryViewModel
    {
        public string Username { get; set; }
        public int TotalPoints { get; set; }
    }
} 