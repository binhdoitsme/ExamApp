using ExamApp.Domain;
using ExamApp.DTO;

namespace ExamApp.Services;


public class ExamService(
    IExamRepository examRepository,
    IQuestionBankRepository questionBankRepository
)
{
    private const int TOTAL_QUESTIONS = 120;
    private const string DEFAULT_DURATION = "10m";

    public async Task<ExamDto> CreateNewExam(UserId userId, CancellationToken cancellationToken = default)
    {
        var unstarted = await examRepository.GetLatestUnstarted(userId, cancellationToken);
        if (unstarted != null)
        {
            var unstartedQuestions = await questionBankRepository.GetQuestionsByIds(
                unstarted.Questions.Select(q => q.SourceQuestionId),
                cancellationToken
            );
            return ExamDto.FromDomain(unstarted, unstartedQuestions);
        }
        // Category weights from the constitution (percentage values)
        var categoryWeights = new Dictionary<string, double>
        {
            { "Requirements Analysis and Design Definition", 30 },
            { "Strategy Analysis", 15 },
            { "Requirements Life Cycle Management", 15 },
            { "Solution Evaluation", 14 },
            { "Business Analysis Planning and Monitoring", 14 },
            { "Elicitation and Collaboration", 12 }
        };

        // Calculate question count per category based on weights
        var questionsPerCategory = categoryWeights
            .ToDictionary(
                kvp => kvp.Key,
                kvp => (int) Math.Round(kvp.Value / 100 * TOTAL_QUESTIONS)
            );

        // Adjust counts to match totalQuestions exactly
        var sumCounts = questionsPerCategory.Values.Sum();
        var diff = TOTAL_QUESTIONS - sumCounts;
        if (diff != 0)
        {
            // Add/subtract difference to the category with highest weight
            var maxCategory = questionsPerCategory.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            questionsPerCategory[maxCategory] += diff;
        }

        // Fetch randomized questions by category from question bank repository
        var selectedQuestionsTasks = questionsPerCategory.Select(async kvp =>
            await questionBankRepository.GetRandomQuestionsByCategory(kvp.Key, kvp.Value, cancellationToken)
        );

        var selectedQuestionsResults = new List<QuestionBankQuestion>();
        foreach (var task in selectedQuestionsTasks)
        {
            selectedQuestionsResults.AddRange(await task);
        }

        var selectedQuestions = selectedQuestionsResults;

        var examId = new ExamId(Guid.NewGuid());
        var examQuestions = selectedQuestions.Shuffle()
            .Select((q, idx) => new ExamQuestion(
                examId,
                idx,
                q.Id,
				[.. q.Answers.Keys.Shuffle()]
			))
            .OrderBy(q => q.Index)
            .ToList();
        var exam = new Exam(Duration.Parse(DEFAULT_DURATION), examQuestions, userId, examId);
        await examRepository.Save(exam, cancellationToken);

        var questions = await questionBankRepository.GetQuestionsByIds(exam.Questions.Select(q => q.SourceQuestionId), cancellationToken);
        return ExamDto.FromDomain(exam, questions);
    }

    public async Task StartExam(ExamId examId, UserId userId, CancellationToken cancellationToken = default)
    {
        var exam = await examRepository.GetById(examId, cancellationToken)
            ?? throw new InvalidOperationException("Exam not found");

        if (exam.CreatedBy != userId)
            throw new UnauthorizedAccessException("Unauthorized");

        if (exam.StartedAt.HasValue)
            throw new InvalidOperationException("Exam already started");

        exam.Start();
        await examRepository.Save(exam, cancellationToken);
    }

    public async Task<ExamDto?> GetExamById(
        ExamId examId,
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        var exam = await examRepository.GetById(examId, cancellationToken);
        if (exam?.CreatedBy != userId)
        {
            return null;
        }
        var questions = await questionBankRepository.GetQuestionsByIds(exam.Questions.Select(q => q.SourceQuestionId), cancellationToken);
        return ExamDto.FromDomain(exam, questions);
    }

    public async Task SubmitExam(
        ExamId examId,
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        var exam = await examRepository.GetById(examId, cancellationToken)
            ?? throw new InvalidOperationException("Exam not found");

        if (exam.CreatedBy != userId)
            throw new UnauthorizedAccessException("Unauthorized");

        if (exam.SubmittedAt.HasValue)
            throw new InvalidOperationException("Exam already submitted");

        exam.Submit();
        var correctAnswers = await questionBankRepository.GetCorrectAnswers(
            exam.Questions.Select(q => q.SourceQuestionId), cancellationToken);
        exam.RevealAnswers(correctAnswers);
        await examRepository.Save(exam, cancellationToken);
    }

    public async Task UpdateExamAnswers(
        ExamId examId,
        UserId userId,
        int?[] answers,
        CancellationToken cancellationToken = default)
    {
        var exam = await examRepository.GetById(examId, cancellationToken)
            ?? throw new KeyNotFoundException($"Exam with id = {examId} not found");
        if (exam.CreatedBy != userId)
        {
            throw new UnauthorizedAccessException("Unauthorized");
        }
        exam.UpdateAnswers(answers);
        await examRepository.Save(exam, cancellationToken);
    }

    public async Task<ExamListDto> ListExams(UserId userId, CancellationToken cancellationToken = default)
    {
        var exams = await examRepository.ListExams(userId, 20, 0, cancellationToken);
        return ExamListDto.FromExams(exams);
    }
}
