using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BorsaTakip.Api.Models;

namespace BorsaTakip.Api.Data
{
    public class BorsaTakipDbContext : IdentityDbContext<ApplicationUser>
    {
        public BorsaTakipDbContext(DbContextOptions<BorsaTakipDbContext> options) : base(options)
        {
        }

        public DbSet<CoinComment> CoinComments { get; set; }  // Buraya ekle
        public DbSet<CommentReply> CommentReplies { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }
        public DbSet<ChatHistory> ChatHistories { get; set; }
        public DbSet<UserCoinHolding> UserCoinHoldings { get; set; }
        public DbSet<UserBadge> UserBadges { get; set; }
        public DbSet<UserFollow> UserFollows { get; set; }

    }
}
