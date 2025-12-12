using Microsoft.EntityFrameworkCore;

namespace ExamApp.Infrastructure.Database;

public record WhitelistedUser(string Email);

public class WhitelistDbContext(DbContextOptions options) : DbContext(options)
{
    public IQueryable<WhitelistedUser> WhitelistedUsers => Set<WhitelistedUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<WhitelistedUser>(qb =>
        {
            qb.ToTable("whitelisted_users").HasKey(u => u.Email);
        });
}
