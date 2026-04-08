namespace DotLearn.Assessment.Models.Entities;

public class QuestionOption
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; } = false;
}
