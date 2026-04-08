namespace DotLearn.Assessment.Models.Entities;

public class QuizAttempt
{
    public Guid Id { get; set; }
    public Guid QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
    public Guid StudentId { get; set; }
    public AttemptStatus Status { get; set; } = AttemptStatus.InProgress;
    public int Score { get; set; } = 0;
    public double Percentage { get; set; } = 0;
    public bool Passed { get; set; } = false;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    public ICollection<AttemptAnswer> Answers { get; set; } = new List<AttemptAnswer>();
}

public enum AttemptStatus { InProgress = 0, Submitted = 1, TimedOut = 2 }
