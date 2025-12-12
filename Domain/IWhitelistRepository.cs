namespace ExamApp.Domain;

public interface IWhitelistRepository
{
    Task<bool> IsWhitelistedUser(string email, CancellationToken cancellationToken = default);
}
