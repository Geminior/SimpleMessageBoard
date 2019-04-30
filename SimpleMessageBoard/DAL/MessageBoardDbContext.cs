namespace SimpleMessageBoard.DAL
{
    using Microsoft.EntityFrameworkCore;
    using SimpleMessageBoard.Model;

    public class MessageBoardDbContext : DbContext
    {
        public MessageBoardDbContext(DbContextOptions<MessageBoardDbContext> options)
            : base(options)
        {
        }

        public DbSet<BoardUser> Users { get; set; }

        public DbSet<BoardMessage> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BoardUser>(ent =>
            {
                ent.HasIndex(u => new { u.UserName })
                .IsUnique();
            });

            modelBuilder.Entity<BoardMessage>(ent =>
            {
                ent.Property(e => e.AuthorId);
                ent.HasOne(e => e.Author);
            });
        }
    }
}
