using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ExamApp.Services;
using ExamApp.Domain;
using ExamApp.DTO;

namespace ExamApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires authentication
public class ExamController(ExamService examService, ILogger<ExamController> logger) : ControllerBase
{
    // [HttpPost]
    // public async Task<ActionResult<Exam>> CreateNewExam(CancellationToken cancellationToken = default)
    // {
    //     try
    //     {
    //         var userId = new UserId(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.NewGuid().ToString());
    //         var exam = await examService.CreateNewExam(userId, cancellationToken);
    //         return Ok(exam);
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, "Failed to create new exam");
    //         return StatusCode(500, "Internal server error");
    //     }
    // }

    [Authorize]
    [HttpPost("{examId}/start")]
    public async Task<IActionResult> StartExam([FromRoute] Guid examId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = new UserId(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.NewGuid().ToString());
            await examService.StartExam(new ExamId(examId), userId, cancellationToken);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized("You are not authorized to start this exam");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start exam {ExamId}", examId);
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize]
    [HttpGet("{examId}")]
    public async Task<ActionResult<Exam>> GetExam([FromRoute] Guid examId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = new UserId(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.NewGuid().ToString());
            var exam = await examService.GetExamById(new ExamId(examId), userId, cancellationToken);
            return exam == null ? NotFound() : Ok(exam);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get exam {ExamId}", examId);
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize]
    [HttpPost("{examId}/submit")]
    public async Task<IActionResult> SubmitExam([FromRoute] Guid examId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = new UserId(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.NewGuid().ToString());
            await examService.SubmitExam(new ExamId(examId), userId, cancellationToken);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized("You are not authorized to submit this exam");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to submit exam {ExamId}", examId);
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize]
    [HttpPost("{examId}/answers")]
    public async Task<IActionResult> UpdateAnswers([FromRoute] Guid examId, [FromBody] int?[] answers, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = new UserId(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.NewGuid().ToString());
            await examService.UpdateExamAnswers(new ExamId(examId), userId, answers, cancellationToken);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update answers for exam {ExamId}", examId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<ExamDto>>> ListExams(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = new UserId(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.NewGuid().ToString());
            var exams = await examService.ListExams(userId, cancellationToken);
            return Ok(exams);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list exams");
            return StatusCode(500, "Internal server error");
        }
    }
}
