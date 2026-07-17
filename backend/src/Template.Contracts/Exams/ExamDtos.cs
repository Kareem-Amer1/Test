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
    int Order,
    string? PartitionId,
    string? PartitionName);

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
    string PositionId,
    string Status,
    DateTime StartedAt,
    DateTime? SubmittedAt,
    int? TotalScore,
    int MaxScore,
    int AutoGradedScore,
    bool IsFullyGraded,
    string ConductedByName);

public sealed record ExamListQueryDto(string? PositionId, string? Status, DateTime? From, DateTime? To, string? Search);

public sealed record ExamAnswerReviewDto(
    string QuestionId,
    string QuestionText,
    string QuestionType,
    int Points,
    int Order,
    IReadOnlyList<ExamSessionChoiceDto>? Choices,
    string CandidateAnswer,
    string CorrectAnswer,
    bool? IsCorrect,
    int? EarnedPoints,
    bool IsAutoGraded,
    string? PartitionId,
    string? PartitionName);

public sealed record ExamDetailResponse(
    string Id,
    string CandidateName,
    string PositionName,
    string Status,
    DateTime StartedAt,
    DateTime? SubmittedAt,
    int? TotalScore,
    int MaxScore,
    int AutoGradedScore,
    bool IsFullyGraded,
    string ConductedByName,
    IReadOnlyList<ExamAnswerReviewDto> Questions);

public sealed record EssayScoreInputDto(string QuestionId, int EarnedPoints);

public sealed record GradeExamRequest(IReadOnlyList<EssayScoreInputDto> EssayScores, bool Finalize);

public sealed record GradeExamResponse(
    string Id,
    string Status,
    int? TotalScore,
    bool IsFullyGraded);

public sealed record ExamSessionResponse(
    string Id,
    string CandidateName,
    string PositionName,
    int DurationMinutes,
    DateTime StartedAt,
    string Status,
    IReadOnlyList<ExamSessionQuestionDto> Questions,
    IReadOnlyList<ExamAnswerInputDto> Answers);

public sealed record SubmitExamResponse(
    string Id,
    string Status,
    DateTime SubmittedAt,
    int AutoGradedScore,
    bool IsFullyGraded,
    int? TotalScore);
