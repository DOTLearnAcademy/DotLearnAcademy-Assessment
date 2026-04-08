using DotLearn.Assessment.Models.DTOs;
using DotLearn.Assessment.Models.Entities;
using DotLearn.Assessment.Repositories;

namespace DotLearn.Assessment.Services;

public class AssessmentService : IAssessmentService
{
    private readonly IAssessmentRepository _repo;

    public AssessmentService(IAssessmentRepository repo)
    {
        _repo = repo;
    }

    public async Task<QuizResponseDto> CreateQuizAsync(CreateQuizRequestDto dto)
    {
        var quiz = new Quiz
        {
            Id = Guid.NewGuid(),
            CourseId = dto.CourseId,
            Title = dto.Title,
            TimeLimitMinutes = dto.TimeLimitMinutes,
            PassingScore = dto.PassingScore,
            MaxAttempts = dto.MaxAttempts
        };
        await _repo.AddQuizAsync(quiz);
        return MapToQuizDto(quiz);
    }

    public async Task<QuestionResponseDto> AddQuestionAsync(Guid quizId, AddQuestionRequestDto dto)
    {
        var quiz = await _repo.GetQuizByIdAsync(quizId)
            ?? throw new KeyNotFoundException("Quiz not found.");

        if (!Enum.TryParse<QuestionType>(dto.Type, true, out var questionType))
            throw new ArgumentException("Invalid question type.");

        var question = new Question
        {
            Id = Guid.NewGuid(),
            QuizId = quizId,
            Text = dto.Text,
            Type = questionType,
            Marks = dto.Marks,
            OrderIndex = dto.OrderIndex,
            Options = dto.Options.Select(o => new QuestionOption
            {
                Id = Guid.NewGuid(),
                Text = o.Text,
                IsCorrect = o.IsCorrect
            }).ToList()
        };

        await _repo.AddQuestionAsync(question);
        return MapToQuestionDto(question);
    }

    public async Task PublishQuizAsync(Guid quizId)
    {
        var quiz = await _repo.GetQuizByIdAsync(quizId)
            ?? throw new KeyNotFoundException("Quiz not found.");
        quiz.IsPublished = true;
        await _repo.UpdateQuizAsync(quiz);
    }

    public async Task<List<QuizResponseDto>> GetPublishedQuizzesByCourseAsync(Guid courseId)
    {
        var quizzes = await _repo.GetPublishedQuizzesByCourseAsync(courseId);
        return quizzes.Select(MapToQuizDto).ToList();
    }

    public async Task<AttemptStartResponseDto> StartAttemptAsync(Guid quizId, Guid studentId)
    {
        var quiz = await _repo.GetQuizWithQuestionsAsync(quizId)
            ?? throw new KeyNotFoundException("Quiz not found.");

        if (!quiz.IsPublished)
            throw new InvalidOperationException("Quiz is not published.");

        var attemptCount = await _repo.GetAttemptCountAsync(quizId, studentId);
        if (attemptCount >= quiz.MaxAttempts)
            throw new InvalidOperationException("Maximum attempts reached.");

        var active = await _repo.GetActiveAttemptAsync(quizId, studentId);
        if (active != null)
        {
            var shuffledExisting = quiz.Questions.OrderBy(_ => Guid.NewGuid()).ToList();
            return MapToStartDto(active, quiz, shuffledExisting);
        }

        var attempt = new QuizAttempt
        {
            Id = Guid.NewGuid(),
            QuizId = quizId,
            StudentId = studentId,
            StartedAt = DateTime.UtcNow,
            Status = AttemptStatus.InProgress
        };

        await _repo.AddAttemptAsync(attempt);
        var shuffled = quiz.Questions.OrderBy(_ => Guid.NewGuid()).ToList();
        return MapToStartDto(attempt, quiz, shuffled);
    }

    public async Task<AttemptResultDto> SubmitAttemptAsync(
        Guid attemptId, SubmitAttemptRequestDto dto, Guid studentId)
    {
        var attempt = await _repo.GetAttemptWithQuizAsync(attemptId)
            ?? throw new KeyNotFoundException("Attempt not found.");

        if (attempt.StudentId != studentId)
            throw new UnauthorizedAccessException("Not your attempt.");

        if (attempt.Status != AttemptStatus.InProgress)
            throw new InvalidOperationException("Attempt already submitted.");

        var elapsed = DateTime.UtcNow - attempt.StartedAt;
        attempt.Status = elapsed.TotalMinutes > attempt.Quiz.TimeLimitMinutes
            ? AttemptStatus.TimedOut
            : AttemptStatus.Submitted;

        int totalMarks = 0, earnedMarks = 0;
        var answers = new List<AttemptAnswer>();

        foreach (var question in attempt.Quiz.Questions)
        {
            totalMarks += question.Marks;
            var correctIds = question.Options
                .Where(o => o.IsCorrect).Select(o => o.Id).ToHashSet();

            var studentAnswer = dto.Answers
                .FirstOrDefault(a => a.QuestionId == question.Id);

            var selectedIds = studentAnswer?.SelectedOptionIds ?? new List<Guid>();

            if (selectedIds.ToHashSet().SetEquals(correctIds))
                earnedMarks += question.Marks;

            answers.Add(new AttemptAnswer
            {
                Id = Guid.NewGuid(),
                AttemptId = attemptId,
                QuestionId = question.Id,
                SelectedOptionIds = selectedIds
            });
        }

        attempt.Score = earnedMarks;
        attempt.Percentage = totalMarks == 0 ? 0 :
            Math.Round((double)earnedMarks / totalMarks * 100, 2);
        attempt.Passed = attempt.Percentage >= attempt.Quiz.PassingScore;
        attempt.SubmittedAt = DateTime.UtcNow;

        await _repo.AddAnswersAsync(answers);
        await _repo.UpdateAttemptAsync(attempt);

        return MapToResultDto(attempt);
    }

    public async Task<AttemptResultDto> GetAttemptResultAsync(Guid attemptId)
    {
        var attempt = await _repo.GetAttemptWithQuizAsync(attemptId)
            ?? throw new KeyNotFoundException("Attempt not found.");
        return MapToResultDto(attempt);
    }

    public async Task<List<AttemptSummaryDto>> GetMyAttemptsAsync(Guid quizId, Guid studentId)
    {
        var attempts = await _repo.GetMyAttemptsAsync(quizId, studentId);
        return attempts.Select(a => new AttemptSummaryDto(
            a.Id, a.Status.ToString(), a.Score, a.Percentage,
            a.Passed, a.StartedAt, a.SubmittedAt)).ToList();
    }

    private static QuizResponseDto MapToQuizDto(Quiz q) => new(
        q.Id, q.CourseId, q.Title, q.TimeLimitMinutes,
        q.PassingScore, q.MaxAttempts, q.IsPublished, q.CreatedAt);

    private static QuestionResponseDto MapToQuestionDto(Question q) => new(
        q.Id, q.QuizId, q.Text, q.Type.ToString(), q.Marks, q.OrderIndex,
        q.Options.Select(o => new OptionResponseDto(o.Id, o.Text)).ToList());

    private static AttemptStartResponseDto MapToStartDto(
        QuizAttempt attempt, Quiz quiz, List<Question> questions) => new(
        attempt.Id, quiz.Id, quiz.TimeLimitMinutes, attempt.StartedAt,
        questions.Select(q => new QuestionResponseDto(
            q.Id, q.QuizId, q.Text, q.Type.ToString(), q.Marks, q.OrderIndex,
            q.Options.Select(o => new OptionResponseDto(o.Id, o.Text)).ToList()
        )).ToList());

    private static AttemptResultDto MapToResultDto(QuizAttempt attempt) => new(
        attempt.Id, attempt.QuizId, attempt.Status.ToString(),
        attempt.Score, attempt.Percentage, attempt.Passed,
        attempt.StartedAt, attempt.SubmittedAt,
        attempt.Quiz.Questions.Select(q =>
        {
            var studentAnswer = attempt.Answers
                .FirstOrDefault(a => a.QuestionId == q.Id);
            var selectedIds = studentAnswer?.SelectedOptionIds ?? new List<Guid>();
            var correctIds = q.Options.Where(o => o.IsCorrect)
                .Select(o => o.Id).ToHashSet();
            return new QuestionResultDto(
                q.Id, q.Text, q.Marks,
                q.Options.Select(o => new OptionWithAnswerDto(
                    o.Id, o.Text, o.IsCorrect)).ToList(),
                selectedIds,
                selectedIds.ToHashSet().SetEquals(correctIds));
        }).ToList());
}
