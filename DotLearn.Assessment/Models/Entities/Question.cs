namespace DotLearn.Assessment.Models.Entities;

public class Question
{
    public Guid Id { get; set; }
    public Guid QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
    public string Text { get; set; } = null!;
    public QuestionType Type { get; set; }
    public int Marks { get; set; } = 1;
    public int OrderIndex { get; set; }
    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
}

public enum QuestionType { SingleChoice = 0, MultipleChoice = 1, TrueFalse = 2 }
