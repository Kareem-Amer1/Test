using HireExam.Application.Common;
using HireExam.Application.Validation;
using HireExam.Contracts.Users;
using HireExam.Core.Entities;
using HireExam.Core.Interfaces;

namespace HireExam.Application.Services;

public interface IUserService
{
    Task<Result<IReadOnlyList<HrUserResponse>>> ListHrUsersAsync(CancellationToken ct = default);
    Task<Result<HrUserResponse>> CreateHrUserAsync(string createdByUserId, CreateHrUserRequest request, CancellationToken ct = default);
    Task<Result<bool>> DeactivateHrUserAsync(string id, string actorUserId, CancellationToken ct = default);
}

public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refresh;
    private readonly IPasswordHasher _hasher;

    public UserService(IUserRepository users, IRefreshTokenRepository refresh, IPasswordHasher hasher)
    {
        _users = users;
        _refresh = refresh;
        _hasher = hasher;
    }

    public async Task<Result<IReadOnlyList<HrUserResponse>>> ListHrUsersAsync(CancellationToken ct = default)
    {
        var items = await _users.ListByRoleAsync(UserRoles.HR, ct);
        var list = items.Select(Map).ToList();
        return Result<IReadOnlyList<HrUserResponse>>.Success(list);
    }

    public async Task<Result<HrUserResponse>> CreateHrUserAsync(
        string createdByUserId,
        CreateHrUserRequest request,
        CancellationToken ct = default)
    {
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!Guard.IsEmail(email))
            return Result<HrUserResponse>.Failure(ErrorCode.Validation, "auth.email_invalid");

        if (!Guard.IsStrongEnoughPassword(request.Password))
            return Result<HrUserResponse>.Failure(ErrorCode.Validation, "auth.password_too_short");

        var fullName = request.FullName?.Trim() ?? string.Empty;
        if (fullName.Length < 2)
            return Result<HrUserResponse>.Failure(ErrorCode.Validation, "users.full_name_required");

        if (await _users.GetByEmailAsync(email, ct) is not null)
            return Result<HrUserResponse>.Failure(ErrorCode.Conflict, "auth.email_taken");

        var user = new User
        {
            Email = email,
            PasswordHash = _hasher.Hash(request.Password),
            FullName = fullName,
            Role = UserRoles.HR,
            IsActive = true,
            CreatedBy = createdByUserId,
        };
        await _users.InsertAsync(user, ct);
        return Result<HrUserResponse>.Success(Map(user));
    }

    public async Task<Result<bool>> DeactivateHrUserAsync(string id, string actorUserId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Result<bool>.Failure(ErrorCode.Validation, "users.id_required");

        if (id == actorUserId)
            return Result<bool>.Failure(ErrorCode.Forbidden, "users.cannot_deactivate_self");

        var user = await _users.GetByIdAsync(id, ct);
        if (user is null)
            return Result<bool>.Failure(ErrorCode.NotFound, "users.not_found");

        if (user.Role != UserRoles.HR)
            return Result<bool>.Failure(ErrorCode.Forbidden, "users.hr_only");

        if (!user.IsActive)
            return Result<bool>.Success(true);

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        var updated = await _users.ReplaceAsync(user, ct);
        if (!updated)
            return Result<bool>.Failure(ErrorCode.NotFound, "users.not_found");

        await _refresh.RevokeAllForUserAsync(user.Id, ct);
        return Result<bool>.Success(true);
    }

    private static HrUserResponse Map(User u) =>
        new(u.Id, u.Email, u.FullName, u.IsActive, u.CreatedAt);
}
