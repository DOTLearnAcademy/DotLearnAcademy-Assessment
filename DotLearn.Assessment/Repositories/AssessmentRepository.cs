using DotLearn.Assessment.Data;
using DotLearn.Assessment.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotLearn.Assessment.Repositories;

public class AssessmentRepository : IAssessmentRepository
{
    private readonly AssessmentDbContext _context;

    public AssessmentRepository(AssessmentDbContext context)
    {
        _context = context;
    }

    public async Task<Quiz?> GetQuizByIdAsync(Guid id) =>
        await _context.Quizzes.FindAsync(id);

    public async Task<Quiz?> GetQuizWithQuestionsAsync(Guid id) =>
        await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == id);

    public async Task<List<Quiz>> GetPublishedQuizzesByCourseAsync(Guid courseId) =>
        await _context.Quizzes
            .Where(q => q.CourseId == courseId && q.IsPublished)
            .OrderBy(q => q.CreatedAt)
            .ToListAsync();

    public async Task AddQuizAsync(Quiz quiz)
    {
        await _context.Quizzes.AddAsync(quiz);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateQuizAsync(Quiz quiz)
    {
        _context.Quizzes.Update(quiz);
        await _context.SaveChangesAsync();
    }

    public async Task AddQuestionAsync(Question question)
    {
        await _context.Questions.AddAsync(question);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetAttemptCountAsync(Guid quizId, Guid studentId) =>
        await _context.QuizAttempts
            .CountAsync(a => a.QuizId == quizId && a.StudentId == studentId);

    public async Task<QuizAttempt?> GetActiveAttemptAsync(Guid quizId, Guid studentId) =>
        await _context.QuizAttempts
            .FirstOrDefaultAsync(a =>
                a.QuizId == quizId &&
                a.StudentId == studentId &&
                a.Status == AttemptStatus.InProgress);

    public async Task<QuizAttempt?> GetAttemptWithQuizAsync(Guid attemptId) =>
        await _context.QuizAttempts
            .Include(a => a.Quiz)
            .ThenInclude(q => q.Questions)
            .ThenInclude(q => q.Options)
            .Include(a => a.Answers)
            .FirstOrDefaultAsync(a => a.Id == attemptId);

    public async Task<List<QuizAttempt>> GetMyAttemptsAsync(Guid quizId, Guid studentId) =>
        await _context.QuizAttempts
            .Where(a => a.QuizId == quizId && a.StudentId == studentId)
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync();

    public async Task AddAttemptAsync(QuizAttempt attempt)
    {
        await _context.QuizAttempts.AddAsync(attempt);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAttemptAsync(QuizAttempt attempt)
    {
        _context.QuizAttempts.Update(attempt);
        await _context.SaveChangesAsync();
    }

    public async Task AddAnswersAsync(List<AttemptAnswer> answers)
    {
        await _context.AttemptAnswers.AddRangeAsync(answers);
        await _context.SaveChangesAsync();
    }
}
