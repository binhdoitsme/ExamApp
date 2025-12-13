namespace ExamApp.Domain;

public class Exam
{
    public ExamId Id { get; }
    public Duration Duration { get; }
    public IReadOnlyList<ExamQuestion> Questions => questions;
    private List<ExamQuestion> questions = [];
    public UserId CreatedBy { get; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public DateTime CreatedAt { get; }

	private Exam() { }

	public Exam(
        Duration duration,
        List<ExamQuestion> questions,
        UserId createdBy,
        ExamId id)
    {
        Id = id;
        Duration = duration;
        this.questions = [.. questions];
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    public Exam(
        ExamId id,
        Duration duration,
        List<ExamQuestion> questions,
        UserId createdBy,
        DateTime? startedAt,
        DateTime? submittedAt,
        DateTime createdAt)
    {
        Id = id;
        Duration = duration;
        this.questions = [.. questions];
        CreatedBy = createdBy;
        StartedAt = startedAt;
        SubmittedAt = submittedAt;
        CreatedAt = createdAt;
    }

    public void Start(DateTime at = default)
    {
        if (StartedAt.HasValue)
            throw new InvalidOperationException("Exam already started");

        StartedAt = at == default ? DateTime.UtcNow : DateTime.SpecifyKind(at, DateTimeKind.Utc);
    }

    public void Submit(DateTime at = default)
    {
        if (SubmittedAt.HasValue)
            throw new InvalidOperationException("Exam already submitted");

        SubmittedAt = at == default ? DateTime.UtcNow : DateTime.SpecifyKind(at, DateTimeKind.Utc);
    }

    public void UpdateAnswers(IEnumerable<int?> answers)
    {
        if (SubmittedAt.HasValue)
            throw new InvalidOperationException("Cannot update a submitted exam");

        var answerList = answers.ToList();
        for (var i = 0; i < answerList.Count && i < Questions.Count; i++)
        {
            if (answerList[i] == null)
            {
                continue;
            }
            Questions[i].UserSelection = answerList[i];
        }
    }

    public void RevealAnswers(
        IList<(int SourceQuestionId, int CorrectAnswer, Dictionary<int, string> Explanations)> questionAnswers)
    {
        if (!SubmittedAt.HasValue)
            throw new InvalidOperationException("Cannot reveal result for an unsubmitted exam");

        var sourceQuestionIdDict = Questions.ToDictionary(
            q => q.SourceQuestionId, q => q
        );
        var mappedAnswers = questionAnswers.ToDictionary(
            qa => qa.SourceQuestionId, qa => qa
        );
        foreach (var q in sourceQuestionIdDict)
        {
            var (SourceQuestionId, CorrectAnswer, Explanations) = mappedAnswers[q.Key];
            q.Value.RevealAnswer(CorrectAnswer, Explanations);
        }
    }
}
