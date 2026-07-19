using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using HireExam.Core.Common;

namespace HireExam.Core.Entities;

[BsonIgnoreExtraElements]
public sealed class ExamQuestionSnapshot
{
    [BsonElement("id")]
    public string Id { get; set; } = string.Empty;

    [BsonElement("type")]
    public string Type { get; set; } = QuestionTypes.Essay;

    [BsonElement("text")]
    public string Text { get; set; } = string.Empty;

    [BsonElement("points")]
    public int Points { get; set; }

    [BsonElement("choices")]
    public List<McqChoice>? Choices { get; set; }

    [BsonElement("correctAnswer")]
    public bool? CorrectAnswer { get; set; }

    [BsonElement("correctChoiceId")]
    public string? CorrectChoiceId { get; set; }

    [BsonElement("partitionId")]
    public string? PartitionId { get; set; }

    [BsonElement("partitionName")]
    public string? PartitionName { get; set; }
}

[BsonIgnoreExtraElements]
public sealed class ExamAnswer
{
    [BsonElement("questionId")]
    public string QuestionId { get; set; } = string.Empty;

    [BsonElement("essayText")]
    public string? EssayText { get; set; }

    [BsonElement("trueFalseAnswer")]
    public bool? TrueFalseAnswer { get; set; }

    [BsonElement("selectedChoiceId")]
    public string? SelectedChoiceId { get; set; }
}

[BsonIgnoreExtraElements]
public sealed class ExamScore
{
    [BsonElement("questionId")]
    public string QuestionId { get; set; } = string.Empty;

    [BsonElement("earnedPoints")]
    public int EarnedPoints { get; set; }

    [BsonElement("isAutoGraded")]
    public bool IsAutoGraded { get; set; }
}

[BsonIgnoreExtraElements]
public sealed class Exam : Entity
{
    [BsonElement("candidateName")]
    public string CandidateName { get; set; } = string.Empty;

    [BsonElement("candidateEmail")]
    public string CandidateEmail { get; set; } = string.Empty;

    [BsonElement("candidateMobile")]
    public string CandidateMobile { get; set; } = string.Empty;

    [BsonElement("invitationId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? InvitationId { get; set; }

    [BsonElement("positionId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string PositionId { get; set; } = string.Empty;

    [BsonElement("positionName")]
    public string PositionName { get; set; } = string.Empty;

    [BsonElement("conductedBy")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ConductedBy { get; set; } = string.Empty;

    [BsonElement("conductedByName")]
    public string ConductedByName { get; set; } = string.Empty;

    [BsonElement("durationMinutes")]
    public int DurationMinutes { get; set; }

    /// <summary>Active exam time consumed (pauses when candidate disconnects).</summary>
    [BsonElement("elapsedSeconds")]
    public int ElapsedSeconds { get; set; }

    [BsonElement("startedAt")]
    public DateTime StartedAt { get; set; }

    [BsonElement("submittedAt")]
    public DateTime? SubmittedAt { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = ExamStatuses.InProgress;

    [BsonElement("questionsSnapshot")]
    public List<ExamQuestionSnapshot> QuestionsSnapshot { get; set; } = new();

    [BsonElement("answers")]
    public List<ExamAnswer> Answers { get; set; } = new();

    [BsonElement("scores")]
    public List<ExamScore> Scores { get; set; } = new();

    [BsonElement("totalScore")]
    public int? TotalScore { get; set; }

    [BsonElement("maxScore")]
    public int MaxScore { get; set; }

    [BsonElement("autoGradedScore")]
    public int AutoGradedScore { get; set; }

    [BsonElement("isFullyGraded")]
    public bool IsFullyGraded { get; set; }
}
