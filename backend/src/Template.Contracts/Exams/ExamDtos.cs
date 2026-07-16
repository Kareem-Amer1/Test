namespace HireExam.Contracts.Exams;

public sealed record CreateExamRequest(string PositionId, string CandidateName);

public sealed record CreateExamResponse(string Id, string CandidateName, string PositionName, string Status);

public sealed record ExamSessionChoiceDto(string Id, string Text);

public sealed record ExamSessionQuestionDto(
    string Id,
    string Type,
    string Text,
    int Points,
    IReadOnlyList<ExamSessionChoiceDto>? Choices,
    int Order);

public sealed record ExamAnswerInputDto(
    string QuestionId,
    string? EssayText,
    bool? TrueFalseAnswer,
    string? SelectedChoiceId);

public sealed record SaveAnswersRequest(IReadOnlyList<ExamAnswerInputDto> Answers);

public sealed record SubmitExamRequest(IReadOnlyList<ExamAnswerInputDto>? Answers);

public sealed record ExamListItemDto(
    string Id,
    string CandidateName,
    string PositionName,
    string Status,
    DateTime StartedAt,
    DateTime? SubmittedAt,
    int? TotalScore,
    int MaxScore,
    string ConductedByName);

public sealed record ExamSessionResponse(
    string Id,
    string CandidateName,
    string PositionName,
    int DurationMinutes,
    DateTime StartedAt,
    string Status,
    IReadOnlyList<ExamSessionQuestionDto> Questions,
    IReadOnlyList<ExamAnswerInputDto> Answers);

public sealed record SubmitExamResponse(string Id, string Status, DateTime SubmittedAt);
