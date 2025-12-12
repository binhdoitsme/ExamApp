namespace ExamApp.Domain;

public enum AnswerLabel
{
    A = 1,
    B = 2,
    C = 3,
    D = 4
}

public class ExamQuestion(
    ExamId examId,
    int index,
    int sourceQuestionId,
    List<int> answerOrder,  // From question bank
    ExamQuestionId? id = null,
    int? userSelection = null,
    int? correctAnswer = null,
    Dictionary<int, string>? explanations = null)
{
    // Question definition
    public ExamQuestionId Id { get; } = id ?? new ExamQuestionId(Guid.NewGuid());
    public ExamId ExamId { get; } = examId;
    public int Index { get; } = index;
    public int SourceQuestionId { get; } = sourceQuestionId;

    // User result data
    public int? UserSelection { get; set; } = userSelection;

    private Dictionary<int, string>? explanations = explanations;

    public bool IsCorrect => CorrectAnswer.HasValue && UserSelection == CorrectAnswer;
    public int? CorrectAnswer { get; private set; } = correctAnswer;
    public IReadOnlyDictionary<int, string>? Explanations => explanations;
    private readonly List<int>? answerOrder = answerOrder;
    public IReadOnlyList<int> AnswerOrder => answerOrder ?? [];

    // Computed AnswerMapping property for domain logic
    public IReadOnlyDictionary<int, AnswerLabel> AnswerMapping
    {
        get
        {
            var result = new Dictionary<int, AnswerLabel>();
            for (var i = 0; i < AnswerOrder.Count; i++)
            {
                result[AnswerOrder[i]] = (AnswerLabel)i;
            }
            return result;
        }
    }

    public void RevealAnswer(int correctAnswer, Dictionary<int, string>? explanations = null)
    {
        CorrectAnswer = correctAnswer;
        this.explanations = explanations;
    }
}
