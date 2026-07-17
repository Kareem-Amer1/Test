using HireExam.Core.Entities;

namespace HireExam.Application.Common;

internal static class TemplateStructure
{
    internal const string LegacyPartitionName = "General";

    internal static bool HasQuestions(ExamTemplate template) =>
        template.Partitions.Any(p => p.Questions.Count > 0) || template.Questions.Count > 0;

    internal static IEnumerable<(TemplatePartition Partition, TemplateQuestion Question)> EnumerateQuestions(
        ExamTemplate template)
    {
        foreach (var partition in template.Partitions.OrderBy(p => p.Order))
        {
            foreach (var question in partition.Questions.OrderBy(q => q.Order))
                yield return (partition, question);
        }
    }

    internal static void EnsurePartitions(ExamTemplate template)
    {
        if (template.Partitions.Count > 0)
            return;

        if (template.Questions.Count == 0)
            return;

        template.Partitions =
        [
            new TemplatePartition
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = LegacyPartitionName,
                Order = 0,
                Questions = template.Questions.OrderBy(q => q.Order).ToList(),
            },
        ];
        template.Questions = new List<TemplateQuestion>();
    }

    internal static TemplatePartition? FindPartition(ExamTemplate template, string partitionId) =>
        template.Partitions.FirstOrDefault(p => p.Id == partitionId);

    internal static (TemplatePartition Partition, int QuestionIndex)? FindQuestion(
        ExamTemplate template, string questionId)
    {
        foreach (var partition in template.Partitions)
        {
            var idx = partition.Questions.FindIndex(q => q.Id == questionId);
            if (idx >= 0)
                return (partition, idx);
        }

        return null;
    }

    internal static void ReindexPartitionOrders(List<TemplatePartition> partitions)
    {
        for (var i = 0; i < partitions.Count; i++)
            partitions[i].Order = i;
    }

    internal static void ReindexQuestionOrders(List<TemplateQuestion> questions)
    {
        for (var i = 0; i < questions.Count; i++)
            questions[i].Order = i;
    }
}
