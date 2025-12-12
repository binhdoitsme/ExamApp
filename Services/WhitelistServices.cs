using ExamApp.Domain;

namespace ExamApp.Services;

public class WhitelistService(IWhitelistRepository whitelistRepository)
{
	public Task<bool> IsWhitelistedUser(
		string email,
		CancellationToken cancellationToken = default
    ) => whitelistRepository.IsWhitelistedUser(email, cancellationToken);
}
