using HireExam.Application.Services;
using HireExam.Core.Entities;
using Xunit;

namespace HireExam.Application.Tests;

public class ExamGradingTests
{
    private static ExamQuestionSnapshot Mcq(string id, int points, string correctId) => new()
    {
        Id = id,
        Type = QuestionTypes.Mcq,
        Text = "MCQ",
        Points = points,
        CorrectChoiceId = correctId,
        Choices = [new McqChoice { Id = "a", Text = "A" }, new McqChoice { Id = correctId, Text = "Correct" }],
    };

    private static ExamQuestionSnapshot TrueFalse(string id, int points, bool correct) => new()
    {
        Id = id,
        Type = QuestionTypes.TrueFalse,
        Text = "TF",
        Points = points,
        CorrectAnswer = correct,
    };

    [Fact]
    public void GradeAutoQuestion_McqCorrect_ReturnsFullPoints()
    {
        var q = Mcq("q1", 10, "c1");
        var answer = new ExamAnswer { QuestionId = "q1", SelectedChoiceId = "c1" };
        Assert.Equal(10, ExamGrading.GradeAutoQuestion(q, answer));
    }

    [Fact]
    public void GradeAutoQuestion_McqWrong_ReturnsZero()
    {
        var q = Mcq("q1", 10, "c1");
        var answer = new ExamAnswer { QuestionId = "q1", SelectedChoiceId = "wrong" };
        Assert.Equal(0, ExamGrading.GradeAutoQuestion(q, answer));
    }

    [Fact]
    public void ApplyAutoGrading_NoEssays_SetsGradedAndTotalScore()
    {
        var exam = new Exam
        {
            QuestionsSnapshot = [TrueFalse("tf1", 5, true), Mcq("mcq1", 15, "c1")],
            Answers =
            [
                new ExamAnswer { QuestionId = "tf1", TrueFalseAnswer = true },
                new ExamAnswer { QuestionId = "mcq1", SelectedChoiceId = "c1" },
            ],
        };

        ExamGrading.ApplyAutoGrading(exam);

        Assert.Equal(ExamStatuses.Graded, exam.Status);
        Assert.True(exam.IsFullyGraded);
        Assert.Equal(20, exam.AutoGradedScore);
        Assert.Equal(20, exam.TotalScore);
        Assert.Equal(2, exam.Scores.Count);
    }

    [Fact]
    public void ApplyAutoGrading_WithEssay_StaysSubmittedUntilEssayGraded()
    {
        var exam = new Exam
        {
            QuestionsSnapshot =
            [
                TrueFalse("tf1", 10, true),
                new ExamQuestionSnapshot { Id = "e1", Type = QuestionTypes.Essay, Text = "Essay", Points = 20 },
            ],
            Answers =
            [
                new ExamAnswer { QuestionId = "tf1", TrueFalseAnswer = true },
                new ExamAnswer { QuestionId = "e1", EssayText = "Answer" },
            ],
        };

        ExamGrading.ApplyAutoGrading(exam);

        Assert.Equal(ExamStatuses.Submitted, exam.Status);
        Assert.False(exam.IsFullyGraded);
        Assert.Equal(10, exam.AutoGradedScore);
        Assert.Null(exam.TotalScore);

        ExamGrading.ApplyEssayScores(exam, [("e1", 18)], finalize: true);

        Assert.Equal(ExamStatuses.Graded, exam.Status);
        Assert.True(exam.IsFullyGraded);
        Assert.Equal(28, exam.TotalScore);
    }
}
