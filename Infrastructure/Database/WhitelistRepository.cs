using ExamApp.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExamApp.Infrastructure.Database;

public class WhitelistRepository(WhitelistDbContext dbContext) : IWhitelistRepository
{
	public async Task<bool> IsWhitelistedUser(
        string email,
        CancellationToken cancellationToken = default
    ) => await dbContext.WhitelistedUsers.AnyAsync(u => u.Email == email, cancellationToken);
}
