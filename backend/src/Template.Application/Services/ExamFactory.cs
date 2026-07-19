using HireExam.Application.Common;
using HireExam.Core.Entities;
using HireExam.Core.Interfaces;

namespace HireExam.Application.Services;

internal static class ExamFactory
{
    public static async Task<(Exam? Exam, string? ErrorCode)> CreateFromTemplateAsync(
        IPositionRepository positions,
        IExamTemplateRepository templates,
        string positionId,
        string conductedBy,
        string conductedByName,
        string candidateName,
        string candidateEmail,
        string candidateMobile,
        string? invitationId,
        CancellationToken ct)
    {
        var position = await positions.GetByIdAsync(positionId, ct);
        if (position is null)
            return (null, "positions.not_found");

        var template = await templates.GetByPositionIdAsync(positionId, ct);
        if (template is null)
            return (null, "exams.template_empty");

        TemplateStructure.EnsurePartitions(template);
        if (!TemplateStructure.HasQuestions(template))
            return (null, "exams.template_empty");

        var snapshot = TemplateStructure.EnumerateQuestions(template)
            .Select(pair => new ExamQuestionSnapshot
            {
                Id = pair.Question.Id,
                Type = pair.Question.Type,
                Text = pair.Question.Text,
                Points = pair.Question.Points,
                Choices = pair.Question.Choices?.Select(c => new McqChoice { Id = c.Id, Text = c.Text }).ToList(),
                CorrectAnswer = pair.Question.CorrectAnswer,
                CorrectChoiceId = pair.Question.CorrectChoiceId,
                PartitionId = pair.Partition.Id,
                PartitionName = pair.Partition.Name,
            })
            .ToList();

        var exam = new Exam
        {
            CandidateName = candidateName,
            CandidateEmail = candidateEmail,
            CandidateMobile = candidateMobile,
            InvitationId = invitationId,
            PositionId = position.Id,
            PositionName = position.Name,
            ConductedBy = conductedBy,
            ConductedByName = conductedByName,
            DurationMinutes = template.DurationMinutes,
            ElapsedSeconds = 0,
            StartedAt = DateTime.UtcNow,
            Status = ExamStatuses.InProgress,
            QuestionsSnapshot = snapshot,
            Answers = snapshot.Select(q => new ExamAnswer { QuestionId = q.Id }).ToList(),
            MaxScore = snapshot.Sum(q => q.Points),
            AutoGradedScore = 0,
            IsFullyGraded = false,
        };

        return (exam, null);
    }
}
