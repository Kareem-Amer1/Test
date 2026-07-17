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
    public void GradeAutoQuestion_TrueFalseWrong_ReturnsZero()
    {
        var q = TrueFalse("tf1", 8, true);
        var answer = new ExamAnswer { QuestionId = "tf1", TrueFalseAnswer = false };
        Assert.Equal(0, ExamGrading.GradeAutoQuestion(q, answer));
    }

    [Fact]
    public void GradeAutoQuestion_NullAnswer_ReturnsZero()
    {
        var q = Mcq("q1", 10, "c1");
        Assert.Equal(0, ExamGrading.GradeAutoQuestion(q, null));
    }

    [Fact]
    public void IsAutoAnswerCorrect_Mcq_ReturnsExpected()
    {
        var q = Mcq("q1", 10, "c1");
        Assert.True(ExamGrading.IsAutoAnswerCorrect(q, new ExamAnswer { SelectedChoiceId = "c1" }));
        Assert.False(ExamGrading.IsAutoAnswerCorrect(q, new ExamAnswer { SelectedChoiceId = "wrong" }));
    }

    [Fact]
    public void ApplyEssayScores_ClampsScoreToMaxPoints()
    {
        var exam = new Exam
        {
            QuestionsSnapshot = [new ExamQuestionSnapshot { Id = "e1", Type = QuestionTypes.Essay, Text = "Essay", Points = 10 }],
            Scores = [],
        };

        ExamGrading.ApplyEssayScores(exam, [("e1", 99)], finalize: false);

        Assert.Equal(10, ExamGrading.GetEarnedPoints(exam, "e1"));
    }

    [Fact]
    public void ApplyEssayScores_WithoutFinalize_DoesNotMarkGraded()
    {
        var exam = new Exam
        {
            Status = ExamStatuses.Submitted,
            QuestionsSnapshot = [new ExamQuestionSnapshot { Id = "e1", Type = QuestionTypes.Essay, Text = "Essay", Points = 10 }],
            Scores = [],
        };

        ExamGrading.ApplyEssayScores(exam, [("e1", 7)], finalize: false);

        Assert.Equal(ExamStatuses.Submitted, exam.Status);
        Assert.False(exam.IsFullyGraded);
        Assert.Null(exam.TotalScore);
    }

    [Fact]
    public void FormatCandidateAnswer_Mcq_ShowsChoiceText()
    {
        var q = Mcq("q1", 5, "c1");
        var text = ExamGrading.FormatCandidateAnswer(q, new ExamAnswer { SelectedChoiceId = "c1" });
        Assert.Equal("Correct", text);
    }

    [Fact]
    public void FormatCorrectAnswer_TrueFalse_ShowsLabel()
    {
        var q = TrueFalse("tf1", 5, true);
        Assert.Equal("True", ExamGrading.FormatCorrectAnswer(q));
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
