using DotLearn.Assessment.Models.Entities;

namespace DotLearn.Assessment.Repositories;

public interface IAssessmentRepository
{
    Task<Quiz?> GetQuizByIdAsync(Guid id);
    Task<Quiz?> GetQuizWithQuestionsAsync(Guid id);
    Task<List<Quiz>> GetPublishedQuizzesByCourseAsync(Guid courseId);
    Task AddQuizAsync(Quiz quiz);
    Task UpdateQuizAsync(Quiz quiz);
    Task AddQuestionAsync(Question question);
    Task<int> GetAttemptCountAsync(Guid quizId, Guid studentId);
    Task<QuizAttempt?> GetActiveAttemptAsync(Guid quizId, Guid studentId);
    Task<QuizAttempt?> GetAttemptWithQuizAsync(Guid attemptId);
    Task<List<QuizAttempt>> GetMyAttemptsAsync(Guid quizId, Guid studentId);
    Task AddAttemptAsync(QuizAttempt attempt);
    Task UpdateAttemptAsync(QuizAttempt attempt);
    Task AddAnswersAsync(List<AttemptAnswer> answers);
}
