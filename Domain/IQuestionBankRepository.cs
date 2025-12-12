namespace ExamApp.Domain;

public interface IQuestionBankRepository
{
    Task<IEnumerable<QuestionBankQuestion>> GetRandomQuestionsByCategory(
        string category, int count, CancellationToken cancellationToken = default);

    Task<IList<(int QuestionId, int CorrectAnswer, Dictionary<int, string> Explanations)>> GetCorrectAnswers(
        IEnumerable<int> questionIds, CancellationToken cancellationToken = default
    );

    Task<IDictionary<int, QuestionBankQuestion>> GetQuestionsByIds(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default
    );
}