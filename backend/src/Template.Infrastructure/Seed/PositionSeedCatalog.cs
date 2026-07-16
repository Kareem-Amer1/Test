using HireExam.Core.Entities;

namespace HireExam.Infrastructure.Seed;

internal static class PositionSeedCatalog
{
    internal sealed record SeedPosition(string Name, string Description, int DurationMinutes, TemplateQuestion[] Questions);

    internal static readonly SeedPosition[] Defaults =
    [
        new(
            "Software Engineer",
            "Technical assessment for software engineering candidates.",
            60,
            [
                new TemplateQuestion
                {
                    Id = "se-q1",
                    Type = QuestionTypes.Essay,
                    Text = "Describe how you would debug an intermittent production issue.",
                    Points = 20,
                    Order = 0,
                },
                new TemplateQuestion
                {
                    Id = "se-q2",
                    Type = QuestionTypes.TrueFalse,
                    Text = "REST APIs are stateless by design.",
                    Points = 10,
                    CorrectAnswer = true,
                    Order = 1,
                },
                new TemplateQuestion
                {
                    Id = "se-q3",
                    Type = QuestionTypes.Mcq,
                    Text = "Which sorting algorithm has average time complexity O(n log n)?",
                    Points = 15,
                    Choices =
                    [
                        new McqChoice { Id = "se-c1", Text = "Bubble Sort" },
                        new McqChoice { Id = "se-c2", Text = "Merge Sort" },
                        new McqChoice { Id = "se-c3", Text = "Selection Sort" },
                    ],
                    CorrectChoiceId = "se-c2",
                    Order = 2,
                },
            ]),
        new(
            "Sales",
            "Assessment for sales and business development roles.",
            45,
            [
                new TemplateQuestion
                {
                    Id = "sa-q1",
                    Type = QuestionTypes.Essay,
                    Text = "How do you handle rejection from a qualified prospect?",
                    Points = 25,
                    Order = 0,
                },
                new TemplateQuestion
                {
                    Id = "sa-q2",
                    Type = QuestionTypes.TrueFalse,
                    Text = "Active listening is essential in consultative selling.",
                    Points = 10,
                    CorrectAnswer = true,
                    Order = 1,
                },
                new TemplateQuestion
                {
                    Id = "sa-q3",
                    Type = QuestionTypes.Mcq,
                    Text = "What is the primary goal of discovery in a sales call?",
                    Points = 15,
                    Choices =
                    [
                        new McqChoice { Id = "sa-c1", Text = "Close immediately" },
                        new McqChoice { Id = "sa-c2", Text = "Understand customer needs" },
                        new McqChoice { Id = "sa-c3", Text = "Present all product features" },
                    ],
                    CorrectChoiceId = "sa-c2",
                    Order = 2,
                },
            ]),
        new(
            "IT Support",
            "Assessment for IT support and helpdesk candidates.",
            30,
            [
                new TemplateQuestion
                {
                    Id = "it-q1",
                    Type = QuestionTypes.Essay,
                    Text = "Walk through how you would troubleshoot a user who cannot connect to the company VPN.",
                    Points = 20,
                    Order = 0,
                },
                new TemplateQuestion
                {
                    Id = "it-q2",
                    Type = QuestionTypes.TrueFalse,
                    Text = "Rebooting a device can resolve many transient software issues.",
                    Points = 10,
                    CorrectAnswer = true,
                    Order = 1,
                },
                new TemplateQuestion
                {
                    Id = "it-q3",
                    Type = QuestionTypes.Mcq,
                    Text = "Which command checks IP configuration on Windows?",
                    Points = 15,
                    Choices =
                    [
                        new McqChoice { Id = "it-c1", Text = "ping" },
                        new McqChoice { Id = "it-c2", Text = "ipconfig" },
                        new McqChoice { Id = "it-c3", Text = "tracert" },
                    ],
                    CorrectChoiceId = "it-c2",
                    Order = 2,
                },
            ]),
    ];
}
