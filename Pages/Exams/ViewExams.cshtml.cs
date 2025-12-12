using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ExamApp.Domain;
using ExamApp.Services;
using ExamApp.DTO;
using System.Security.Claims;

namespace ExamApp.Pages.Exams;

[Authorize]
public class ListModel(ExamService examService) : PageModel
{
	public ExamListDto Exams { get; set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
        Exams = await examService.ListExams(new UserId(userId), cancellationToken);
    }
}
