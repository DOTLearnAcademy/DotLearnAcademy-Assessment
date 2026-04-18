using DotLearn.Assessment.Models.DTOs;

namespace DotLearn.Assessment.Services;

public interface IAssessmentService
{
    Task<QuizResponseDto> CreateQuizAsync(CreateQuizRequestDto dto);
    Task<QuestionResponseDto> AddQuestionAsync(Guid quizId, AddQuestionRequestDto dto);
    Task PublishQuizAsync(Guid quizId);
    Task<List<QuizResponseDto>> GetPublishedQuizzesByCourseAsync(Guid courseId);
    Task<AttemptStartResponseDto> StartAttemptAsync(Guid quizId, Guid studentId);
    Task<AttemptResultDto> SubmitAttemptAsync(Guid attemptId, SubmitAttemptRequestDto dto, Guid studentId);
    Task<AttemptResultDto> GetAttemptResultAsync(Guid attemptId);
    Task<List<AttemptSummaryDto>> GetMyAttemptsAsync(Guid quizId, Guid studentId);
}
