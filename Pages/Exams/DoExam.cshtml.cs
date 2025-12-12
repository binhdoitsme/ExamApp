using ExamApp.Domain;
using ExamApp.DTO;
using ExamApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace ExamApp.Pages.Exams;

[Authorize]
public class DoExamModel(ExamService examService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid ExamId { get; set; }

    public ExamDto? Exam { get; set; }
    public TimeSpan? TimeRemaining { get; set; }
    public bool IsExpired { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
        var exam = await examService.GetExamById(new ExamId(ExamId), new UserId(userId));

        if (exam == null)
            return NotFound();

        Exam = exam;

        // Calculate remaining time
        if (exam.StartedAt.HasValue)
        {
            var duration = Duration.Parse(exam.Duration);
            var endTime = exam.StartedAt.Value.Add(duration.Value);
            TimeRemaining = endTime - DateTime.UtcNow;

            if (TimeRemaining.Value <= TimeSpan.Zero)
            {
                IsExpired = true;
                TimeRemaining = TimeSpan.Zero;
                // TODO: Auto submit if not already
                if (exam.SubmittedAt == null)
                {
                    await examService.SubmitExam(new ExamId(exam.Id), new UserId(exam.CreatedBy));
                    return LocalRedirect($"/Exams/Results/{exam.Id}");
                }
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSubmitAsync(int?[] answers)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
        await examService.UpdateExamAnswers(new ExamId(ExamId), new UserId(userId), answers);
        await examService.SubmitExam(new ExamId(ExamId), new UserId(userId));
        return RedirectToPage("/Exams/Results", new { examId = ExamId });
    }
}
