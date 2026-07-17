using Moq;
using HireExam.Application.Common;
using HireExam.Application.Services;
using HireExam.Contracts.Templates;
using HireExam.Core.Entities;
using HireExam.Core.Interfaces;
using Xunit;

namespace HireExam.Application.Tests;

public class TemplateServiceTests
{
    private const string PositionId = "674222123456789012345678";
    private const string UserId = "674abc123456789012345678";

    private static (TemplateService svc, Mock<IExamTemplateRepository> templates, ExamTemplate template) Build()
    {
        var positions = new Mock<IPositionRepository>();
        var templates = new Mock<IExamTemplateRepository>();

        positions.Setup(p => p.GetByIdAsync(PositionId, default)).ReturnsAsync(new Position
        {
            Id = PositionId,
            Name = "Engineer",
        });

        var template = new ExamTemplate
        {
            Id = "674333123456789012345678",
            PositionId = PositionId,
            DurationMinutes = 60,
            Partitions =
            [
                new TemplatePartition
                {
                    Id = "part-1",
                    Name = "Soft Skills",
                    Order = 0,
                    Questions =
                    [
                        new TemplateQuestion
                        {
                            Id = "q1",
                            Type = QuestionTypes.Essay,
                            Text = "Tell us about yourself.",
                            Points = 10,
                            Order = 0,
                        },
                    ],
                },
                new TemplatePartition
                {
                    Id = "part-2",
                    Name = ".NET",
                    Order = 1,
                    Questions =
                    [
                        new TemplateQuestion
                        {
                            Id = "q2",
                            Type = QuestionTypes.TrueFalse,
                            Text = "C# is object-oriented.",
                            Points = 5,
                            CorrectAnswer = true,
                            Order = 0,
                        },
                        new TemplateQuestion
                        {
                            Id = "q3",
                            Type = QuestionTypes.Mcq,
                            Text = "Pick one.",
                            Points = 5,
                            Choices =
                            [
                                new McqChoice { Id = "c1", Text = "A" },
                                new McqChoice { Id = "c2", Text = "B" },
                            ],
                            CorrectChoiceId = "c1",
                            Order = 1,
                        },
                    ],
                },
            ],
        };

        templates.Setup(t => t.GetByPositionIdAsync(PositionId, default)).ReturnsAsync(template);
        templates.Setup(t => t.ReplaceAsync(It.IsAny<ExamTemplate>(), default))
            .ReturnsAsync(true);

        var svc = new TemplateService(positions.Object, templates.Object);
        return (svc, templates, template);
    }

    [Fact]
    public async Task DeletePartition_RemovesPartitionAndQuestions()
    {
        var (svc, templates, template) = Build();

        var result = await svc.DeletePartitionAsync(PositionId, UserId, "part-2");

        Assert.True(result.IsSuccess);
        Assert.Single(template.Partitions);
        Assert.Equal("part-1", template.Partitions[0].Id);
        templates.Verify(t => t.ReplaceAsync(template, default), Times.Once);
    }

    [Fact]
    public async Task AddQuestion_AppendsToSelectedPartition()
    {
        var (svc, _, template) = Build();

        var result = await svc.AddQuestionAsync(
            PositionId,
            UserId,
            "part-1",
            new UpsertQuestionRequest(QuestionTypes.TrueFalse, "New question?", 5, true, null, null));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, template.Partitions[0].Questions.Count);
    }

    [Fact]
    public async Task Get_MigratesLegacyFlatQuestionsIntoPartition()
    {
        var positions = new Mock<IPositionRepository>();
        var templates = new Mock<IExamTemplateRepository>();

        positions.Setup(p => p.GetByIdAsync(PositionId, default)).ReturnsAsync(new Position { Id = PositionId });

        var legacy = new ExamTemplate
        {
            Id = "legacy",
            PositionId = PositionId,
            Questions =
            [
                new TemplateQuestion
                {
                    Id = "legacy-q1",
                    Type = QuestionTypes.Essay,
                    Text = "Legacy question",
                    Points = 10,
                    Order = 0,
                },
            ],
        };

        templates.Setup(t => t.GetByPositionIdAsync(PositionId, default)).ReturnsAsync(legacy);

        var svc = new TemplateService(positions.Object, templates.Object);
        var result = await svc.GetByPositionIdAsync(PositionId);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Partitions);
        Assert.Equal("General", result.Value.Partitions[0].Name);
        Assert.Single(result.Value.Partitions[0].Questions);
    }
}
