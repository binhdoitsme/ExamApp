using ExamApp.Domain;

namespace ExamApp.DTO;

public struct ExamBasicDto
{
    public string Id { get; set; }
    public string Duration { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public int QuestionCount { get; set; }
    public int? CorrectCount { get; set; }
}

public struct ExamListDto
{
    public List<ExamBasicDto> Exams { get; set; }

    public static ExamListDto FromExams(IEnumerable<Exam> exams) => new()
    {
        Exams = [
            ..exams.Select(e => new ExamBasicDto
            {
                Id = e.Id.Value.ToString(),
                Duration = e.Duration.ToString(),
                CreatedBy = e.CreatedBy.ToString(),
                CreatedAt = e.CreatedAt,
                StartedAt = e.StartedAt,
                SubmittedAt = e.SubmittedAt,
                QuestionCount = e.Questions.Count,
                CorrectCount = e.SubmittedAt != null ? e.Questions.Where(q => q.IsCorrect).Count() : null,
            })
        ],
    };
}
