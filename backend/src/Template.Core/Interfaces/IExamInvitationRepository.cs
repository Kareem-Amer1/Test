using HireExam.Core.Entities;

namespace HireExam.Core.Interfaces;

public interface IExamInvitationRepository
{
    Task<ExamInvitation?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task<ExamInvitation?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<ExamInvitation>> ListByCreatorAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<ExamInvitation>> ListAllAsync(CancellationToken ct = default);
    Task InsertAsync(ExamInvitation invitation, CancellationToken ct = default);
    Task<bool> ReplaceAsync(ExamInvitation invitation, CancellationToken ct = default);
}
