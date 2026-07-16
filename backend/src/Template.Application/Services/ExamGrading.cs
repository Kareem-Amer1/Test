using HireExam.Core.Entities;

namespace HireExam.Application.Services;

public static class ExamGrading
{
    public static void ApplyAutoGrading(Exam exam)
    {
        var autoScores = exam.QuestionsSnapshot
            .Where(q => q.Type != QuestionTypes.Essay)
            .Select(q =>
            {
                var answer = exam.Answers.FirstOrDefault(a => a.QuestionId == q.Id);
                var earned = GradeAutoQuestion(q, answer);
                return new ExamScore
                {
                    QuestionId = q.Id,
                    EarnedPoints = earned,
                    IsAutoGraded = true,
                };
            })
            .ToList();

        exam.Scores = exam.Scores.Where(s => !s.IsAutoGraded).ToList();
        exam.Scores.AddRange(autoScores);
        exam.AutoGradedScore = autoScores.Sum(s => s.EarnedPoints);

        var hasEssays = exam.QuestionsSnapshot.Any(q => q.Type == QuestionTypes.Essay);
        if (!hasEssays)
        {
            exam.TotalScore = exam.AutoGradedScore;
            exam.IsFullyGraded = true;
            exam.Status = ExamStatuses.Graded;
            return;
        }

        exam.TotalScore = null;
        exam.IsFullyGraded = false;
        exam.Status = ExamStatuses.Submitted;
    }

    public static int GradeAutoQuestion(ExamQuestionSnapshot question, ExamAnswer? answer)
    {
        return question.Type switch
        {
            QuestionTypes.Mcq when answer?.SelectedChoiceId == question.CorrectChoiceId => question.Points,
            QuestionTypes.TrueFalse when answer?.TrueFalseAnswer == question.CorrectAnswer => question.Points,
            QuestionTypes.Mcq or QuestionTypes.TrueFalse => 0,
            _ => 0,
        };
    }

    public static bool? IsAutoAnswerCorrect(ExamQuestionSnapshot question, ExamAnswer? answer)
    {
        return question.Type switch
        {
            QuestionTypes.Mcq => answer?.SelectedChoiceId == question.CorrectChoiceId,
            QuestionTypes.TrueFalse => answer?.TrueFalseAnswer == question.CorrectAnswer,
            _ => null,
        };
    }

    public static void ApplyEssayScores(Exam exam, IReadOnlyList<(string QuestionId, int EarnedPoints)> essayScores, bool finalize)
    {
        var essayIds = exam.QuestionsSnapshot
            .Where(q => q.Type == QuestionTypes.Essay)
            .Select(q => q.Id)
            .ToHashSet();

        foreach (var (questionId, earnedPoints) in essayScores)
        {
            if (!essayIds.Contains(questionId))
                continue;

            var question = exam.QuestionsSnapshot.First(q => q.Id == questionId);
            var clamped = Math.Clamp(earnedPoints, 0, question.Points);
            var existing = exam.Scores.FirstOrDefault(s => s.QuestionId == questionId && !s.IsAutoGraded);
            if (existing is null)
            {
                exam.Scores.Add(new ExamScore
                {
                    QuestionId = questionId,
                    EarnedPoints = clamped,
                    IsAutoGraded = false,
                });
            }
            else
            {
                existing.EarnedPoints = clamped;
            }
        }

        var allEssaysGraded = essayIds.All(id =>
            exam.Scores.Any(s => s.QuestionId == id && !s.IsAutoGraded));

        if (finalize && allEssaysGraded && essayIds.Count > 0)
        {
            exam.TotalScore = exam.Scores.Sum(s => s.EarnedPoints);
            exam.IsFullyGraded = true;
            exam.Status = ExamStatuses.Graded;
        }
    }

    public static int? GetEarnedPoints(Exam exam, string questionId) =>
        exam.Scores.FirstOrDefault(s => s.QuestionId == questionId)?.EarnedPoints;

    public static string FormatCandidateAnswer(ExamQuestionSnapshot question, ExamAnswer? answer)
    {
        if (answer is null) return "—";

        return question.Type switch
        {
            QuestionTypes.Essay => string.IsNullOrWhiteSpace(answer.EssayText) ? "—" : answer.EssayText.Trim(),
            QuestionTypes.TrueFalse => answer.TrueFalseAnswer switch
            {
                true => "True",
                false => "False",
                _ => "—",
            },
            QuestionTypes.Mcq => question.Choices?.FirstOrDefault(c => c.Id == answer.SelectedChoiceId)?.Text ?? "—",
            _ => "—",
        };
    }

    public static string FormatCorrectAnswer(ExamQuestionSnapshot question)
    {
        return question.Type switch
        {
            QuestionTypes.Essay => "—",
            QuestionTypes.TrueFalse => question.CorrectAnswer switch
            {
                true => "True",
                false => "False",
                _ => "—",
            },
            QuestionTypes.Mcq => question.Choices?.FirstOrDefault(c => c.Id == question.CorrectChoiceId)?.Text ?? "—",
            _ => "—",
        };
    }
}
