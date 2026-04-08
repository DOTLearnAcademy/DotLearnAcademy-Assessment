using System.Text.Json;

namespace DotLearn.Assessment.Models.Entities;

public class AttemptAnswer
{
    public Guid Id { get; set; }
    public Guid AttemptId { get; set; }
    public QuizAttempt Attempt { get; set; } = null!;
    public Guid QuestionId { get; set; }
    // Stored as JSON string in DB
    public string SelectedOptionIdsJson { get; set; } = "[]";

    public List<Guid> SelectedOptionIds
    {
        get => JsonSerializer.Deserialize<List<Guid>>(SelectedOptionIdsJson) ?? new();
        set => SelectedOptionIdsJson = JsonSerializer.Serialize(value);
    }
}
