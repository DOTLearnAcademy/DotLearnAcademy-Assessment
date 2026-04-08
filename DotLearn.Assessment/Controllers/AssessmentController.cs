using DotLearn.Assessment.Models.DTOs;
using DotLearn.Assessment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DotLearn.Assessment.Controllers;

[ApiController]
public class AssessmentController : ControllerBase
{
    private readonly IAssessmentService _service;

    public AssessmentController(IAssessmentService service)
    {
        _service = service;
    }

    // POST /api/quizzes
    [HttpPost("api/quizzes")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizRequestDto dto)
    {
        var result = await _service.CreateQuizAsync(dto);
        return StatusCode(201, result);
    }

    // POST /api/quizzes/{id}/questions
    [HttpPost("api/quizzes/{id}/questions")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> AddQuestion(Guid id, [FromBody] AddQuestionRequestDto dto)
    {
        try
        {
            var result = await _service.AddQuestionAsync(id, dto);
            return StatusCode(201, result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // PUT /api/quizzes/{id}/publish
    [HttpPut("api/quizzes/{id}/publish")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> PublishQuiz(Guid id)
    {
        try
        {
            await _service.PublishQuizAsync(id);
            return Ok(new { message = "Quiz published." });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    // GET /api/quizzes/course/{courseId}
    [HttpGet("api/quizzes/course/{courseId}")]
    [Authorize]
    public async Task<IActionResult> GetByCourse(Guid courseId)
    {
        var result = await _service.GetPublishedQuizzesByCourseAsync(courseId);
        return Ok(result);
    }

    // POST /api/quizzes/{id}/attempts/start
    [HttpPost("api/quizzes/{id}/attempts/start")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> StartAttempt(Guid id)
    {
        try
        {
            var result = await _service.StartAttemptAsync(id, GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // POST /api/attempts/{id}/submit
    [HttpPost("api/attempts/{id}/submit")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> SubmitAttempt(Guid id, [FromBody] SubmitAttemptRequestDto dto)
    {
        try
        {
            var result = await _service.SubmitAttemptAsync(id, dto, GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    // GET /api/attempts/{id}/result
    [HttpGet("api/attempts/{id}/result")]
    [Authorize]
    public async Task<IActionResult> GetResult(Guid id)
    {
        try
        {
            var result = await _service.GetAttemptResultAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    // GET /api/quizzes/{id}/attempts/my
    [HttpGet("api/quizzes/{id}/attempts/my")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyAttempts(Guid id)
    {
        var result = await _service.GetMyAttemptsAsync(id, GetUserId());
        return Ok(result);
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found."));
}
