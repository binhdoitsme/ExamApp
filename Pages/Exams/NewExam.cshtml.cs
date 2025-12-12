using ExamApp.Domain;
using ExamApp.DTO;
using ExamApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace ExamApp.Pages.Exams;

public class NewExamModel(
    ExamService examService,
    ILogger<NewExamModel> logger
) : PageModel
{
    public ExamDto? Exam { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = new UserId(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.NewGuid().ToString());
            // TODO: Get the latest unstarted exam - if any then return right away
            Exam = await examService.CreateNewExam(userId, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create new exam");
        }
    }
}
