using DotLearn.Assessment.Models.Entities;
using DotLearn.Assessment.Models.DTOs;
using DotLearn.Assessment.Repositories;
using DotLearn.Assessment.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DotLearn.Assessment.Tests;

[TestClass]
public class AssessmentServiceTests
{
    private Mock<IAssessmentRepository> _repoMock = null!;
    private IAssessmentService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _repoMock = new Mock<IAssessmentRepository>();
        _service = new AssessmentService(_repoMock.Object);
    }

    [TestMethod]
    public async Task SubmitAttemptAsync_AllCorrect_Returns100Percent()
    {
        var optionId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var attemptId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        var attempt = new QuizAttempt
        {
            Id = attemptId,
            StudentId = studentId,            // must match the caller's studentId
            Status = AttemptStatus.InProgress,
            StartedAt = DateTime.UtcNow,
            Answers = new List<AttemptAnswer>(),
            Quiz = new Quiz
            {
                TimeLimitMinutes = 30,
                PassingScore = 70,
                Questions = new List<Question>
                {
                    new()
                    {
                        Id = questionId, Marks = 1,
                        Options = new List<QuestionOption>
                        {
                            new() { Id = optionId, IsCorrect = true }
                        }
                    }
                }
            }
        };

        _repoMock.Setup(r => r.GetAttemptWithQuizAsync(attemptId)).ReturnsAsync(attempt);
        _repoMock.Setup(r => r.AddAnswersAsync(It.IsAny<List<AttemptAnswer>>()))
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.UpdateAttemptAsync(It.IsAny<QuizAttempt>()))
            .Returns(Task.CompletedTask);

        var dto = new SubmitAttemptRequestDto(new List<SubmitAnswerDto>
        {
            new(questionId, new List<Guid> { optionId })
        });

        var result = await _service.SubmitAttemptAsync(attemptId, dto, studentId);

        Assert.AreEqual(100.0, result.Percentage);
        Assert.IsTrue(result.Passed);
    }

    [TestMethod]
    public async Task SubmitAttemptAsync_AlreadySubmitted_ThrowsInvalidOperation()
    {
        var attemptId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        _repoMock.Setup(r => r.GetAttemptWithQuizAsync(attemptId))
            .ReturnsAsync(new QuizAttempt
            {
                Id = attemptId,
                StudentId = studentId,
                Status = AttemptStatus.Submitted,
                Answers = new List<AttemptAnswer>(),
                Quiz = new Quiz { Questions = new List<Question>() }
            });

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
            _service.SubmitAttemptAsync(attemptId,
                new SubmitAttemptRequestDto(new List<SubmitAnswerDto>()), studentId));
    }
}
