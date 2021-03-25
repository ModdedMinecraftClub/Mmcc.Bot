using Microsoft.EntityFrameworkCore;
using Mmcc.Bot.Database.Entities;

namespace Mmcc.Bot.Database
{
    /// <inheritdoc />
    public class BotContext : DbContext
    {
        /// <summary>
        /// Member applications.
        /// </summary>
        public DbSet<MemberApplication> MemberApplications { get; set; } = null!;
        
        /// <summary>
        /// Moderation actions.
        /// </summary>
        public DbSet<ModerationAction> ModerationActions { get; set; } = null!;

        /// <summary>
        /// Tags.
        /// </summary>
        public DbSet<Tag> Tags { get; set; } = null!;
        
        /// <inheritdoc />
        public BotContext(DbContextOptions<BotContext> options)
            : base(options)
        {
        }
        
        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MemberApplication>(e =>
            {
                e.HasIndex(a => a.GuildId);
                e.HasIndex(a => a.AuthorDiscordId);

                e.Property(a => a.AppStatus)
                    .HasColumnType("int(1)");
            });

            modelBuilder.Entity<ModerationAction>(e =>
            {
                e.HasIndex(m => m.ModerationActionType);
                e.HasIndex(m => m.UserDiscordId);
                e.HasIndex(m => m.UserIgn);

                e.Property(m => m.ModerationActionType)
                    .HasColumnType("int(1)");
            });
            
            modelBuilder.Entity<Tag>(e =>
            {
                e.HasKey(t => new {t.GuildId, t.TagName});
                
                e.HasIndex(t => t.GuildId);
                e.HasIndex(t => t.TagName);
            });
        }
    }
}