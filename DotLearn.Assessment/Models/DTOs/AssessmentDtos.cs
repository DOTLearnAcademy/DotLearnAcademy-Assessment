namespace DotLearn.Assessment.Models.DTOs;

public record CreateQuizRequestDto(
    Guid CourseId,
    string Title,
    int TimeLimitMinutes,
    int PassingScore,
    int MaxAttempts
);

public record AddQuestionRequestDto(
    string Text,
    string Type,
    int Marks,
    int OrderIndex,
    List<AddOptionDto> Options
);

public record AddOptionDto(string Text, bool IsCorrect);

public record QuizResponseDto(
    Guid Id,
    Guid CourseId,
    string Title,
    int TimeLimitMinutes,
    int PassingScore,
    int MaxAttempts,
    bool IsPublished,
    DateTime CreatedAt
);

public record QuestionResponseDto(
    Guid Id,
    Guid QuizId,
    string Text,
    string Type,
    int Marks,
    int OrderIndex,
    List<OptionResponseDto> Options
);

public record OptionResponseDto(Guid Id, string Text);

public record OptionWithAnswerDto(Guid Id, string Text, bool IsCorrect);

public record AttemptStartResponseDto(
    Guid AttemptId,
    Guid QuizId,
    int TimeLimitMinutes,
    DateTime StartedAt,
    List<QuestionResponseDto> Questions
);

public record SubmitAttemptRequestDto(List<SubmitAnswerDto> Answers);

public record SubmitAnswerDto(Guid QuestionId, List<Guid> SelectedOptionIds);

public record AttemptResultDto(
    Guid AttemptId,
    Guid QuizId,
    string Status,
    int Score,
    double Percentage,
    bool Passed,
    DateTime StartedAt,
    DateTime? SubmittedAt,
    List<QuestionResultDto> Questions
);

public record QuestionResultDto(
    Guid QuestionId,
    string Text,
    int Marks,
    List<OptionWithAnswerDto> Options,
    List<Guid> SelectedOptionIds,
    bool IsCorrect
);

public record AttemptSummaryDto(
    Guid AttemptId,
    string Status,
    int Score,
    double Percentage,
    bool Passed,
    DateTime StartedAt,
    DateTime? SubmittedAt
);
