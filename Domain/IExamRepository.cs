namespace ExamApp.Domain;

public interface IExamRepository
{
    Task<IEnumerable<Exam>> ListExams(UserId userId, int limit, int offset, CancellationToken cancellationToken = default);
    Task<Exam?> GetLatestUnstarted(UserId userId, CancellationToken cancellationToken = default);
    Task<Exam?> GetById(ExamId id, CancellationToken cancellationToken = default);
    Task Save(Exam exam, CancellationToken cancellationToken = default);
}
