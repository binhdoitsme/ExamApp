using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ExamApp.Services;
using Microsoft.AspNetCore.Mvc;
using ExamApp.DTO;
using ExamApp.Domain;
using System.Security.Claims;

namespace ExamApp.Pages.Exams;

[Authorize]
public class ResultsModel(ExamService examService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid ExamId { get; set; }

    public ExamDto? Exam { get; set; }
    public int CorrectCount { get; set; }
    public int TotalQuestions { get; set; }
    public double ScorePercentage { get; set; }
    public string ResultStatus { get; set; } = string.Empty;
    public string ResultMessage { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
        var exam = await examService.GetExamById(new ExamId(ExamId), new UserId(userId));

        if (exam == null)
            return NotFound();

        Exam = exam;
        TotalQuestions = exam.Questions.Count;
        CorrectCount = exam.Questions.Count(q => q.IsCorrect);
        ScorePercentage = (double) CorrectCount / TotalQuestions * 100;
        ResultStatus = ScorePercentage >= 85 ? "PASSED" : "FAILED";
        ResultMessage = ScorePercentage >= 85
            ? $"Congratulations! You passed with {CorrectCount}/{TotalQuestions} correct ({ScorePercentage:F1}%)"
            : $"You scored {CorrectCount}/{TotalQuestions} correct ({ScorePercentage:F2}%). Try again!";

        return Page();
    }
}
