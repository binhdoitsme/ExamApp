using ExamApp.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExamApp.Infrastructure.Database;

public class QuestionBankRepository(QuestionBankDbContext dbContext) : IQuestionBankRepository
{
    public async Task<IList<(int QuestionId, int CorrectAnswer, Dictionary<int, string> Explanations)>> GetCorrectAnswers(
        IEnumerable<int> questionIds,
        CancellationToken cancellationToken = default
    )
    {
        var questions = await dbContext.Questions
            .Where(q => questionIds.Contains(q.Id))
            .ToListAsync(cancellationToken);
        return questions
            .Select(q => (
                q.Id,
                q.Answers.First(a => a.IsCorrect).Id,
                q.Answers.ToDictionary(a => a.Id, a => a.Explanation ?? "")))
            .ToList();
    }

    public async Task<IEnumerable<QuestionBankQuestion>> GetRandomQuestionsByCategory(
        string category,
        int count,
        CancellationToken cancellationToken = default)
    {
        var questions = await dbContext.Questions
            .Where(q => q.Category == category && q.Answers.Count > 0)
            .OrderBy(q => EF.Functions.Random())  // PostgreSQL RANDOM()
            .Take(count)
            .ToListAsync(cancellationToken);

        return questions.Select(q => new QuestionBankQuestion(
            q.Id,
            q.Text,
            q.Category,
            q.Answers.ToDictionary(a => a.Id, a => a.Text),
            q.ImageDataUri
        ));
    }

    public async Task<IDictionary<int, QuestionBankQuestion>> GetQuestionsByIds(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default
    )
    {
        ids = [.. ids];
        return await dbContext.Questions.Where(q => ids.Contains(q.Id))
            .ToDictionaryAsync(
                q => q.Id,
                q => new QuestionBankQuestion(
                    q.Id,
                    q.Text,
                    q.Category,
                    q.Answers.ToDictionary(a => a.Id, a => a.Text),
                    q.ImageDataUri
                ),
                cancellationToken
            );
    }
}
