namespace DotLearn.Assessment.Models.Entities;

public class Quiz
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = null!;
    public int TimeLimitMinutes { get; set; } = 30;
    public int PassingScore { get; set; } = 70;
    public int MaxAttempts { get; set; } = 3;
    public bool IsPublished { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
