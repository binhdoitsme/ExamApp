using ExamApp.Domain;

namespace ExamApp.DTO;

public class ExamDto
{
    public Guid Id { get; set; }
    public string Duration { get; set; } = null!;
    public List<ExamQuestionDto> Questions { get; set; } = [];
    public string CreatedBy { get; set; } = null!; // UserId as string
    public DateTime? StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Creates ExamDto from domain Exam instance
    /// </summary>
    public static ExamDto FromDomain(
        Exam exam,
        IDictionary<int, QuestionBankQuestion> questionBankQuestions
    ) => new()
    {
        Id = exam.Id.Value,
        Duration = exam.Duration.ToString(),
        Questions = [.. exam.Questions.Select(
            q => ExamQuestionDto.FromDomain(q, questionBankQuestions[q.SourceQuestionId])
        )],
        CreatedBy = exam.CreatedBy.Value,
        StartedAt = exam.StartedAt,
        SubmittedAt = exam.SubmittedAt,
        CreatedAt = exam.CreatedAt
    };
}

public class ExamQuestionDto
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public int Index { get; set; }
    public string Text { get; set; } = null!;
    public Dictionary<int, string> Answers { get; set; } = []; // answerId -> answer text
    public int SourceQuestionId { get; set; }
    public string? SourceCategory { get; set; }
    public string? AttachmentImg { get; set; }

    // User-answer related fields
    public int? UserSelection { get; set; }
    public int? CorrectAnswer { get; set; }
    public Dictionary<int, string>? Explanations { get; set; }
    public bool IsCorrect => UserSelection != null && UserSelection == CorrectAnswer;
    public List<int> AnswerOrder { get; set; } = [];

    /// <summary>
    /// Creates ExamQuestionDto from domain ExamQuestion instance
    /// </summary>
    public static ExamQuestionDto FromDomain(
        ExamQuestion examQuestion,
        QuestionBankQuestion questionBankQuestion
    ) => new()
    {
        Id = examQuestion.Id.Value,
        ExamId = examQuestion.ExamId.Value,
        Index = examQuestion.Index,
        SourceQuestionId = examQuestion.SourceQuestionId,
        Text = questionBankQuestion.Text,
        Answers = questionBankQuestion.Answers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
        SourceCategory = questionBankQuestion.Category,
        AttachmentImg = questionBankQuestion.AttachmentImg,
        UserSelection = examQuestion.UserSelection,
        CorrectAnswer = examQuestion.CorrectAnswer,
        Explanations = examQuestion.Explanations?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
        AnswerOrder = [.. examQuestion.AnswerOrder]
	};
}