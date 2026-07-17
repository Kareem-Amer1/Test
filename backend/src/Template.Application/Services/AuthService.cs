using HireExam.Application.Common;
using HireExam.Application.Validation;
using HireExam.Contracts.Auth;
using HireExam.Core.Entities;
using HireExam.Core.Interfaces;

namespace HireExam.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refresh;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;

    public AuthService(
        IUserRepository users,
        IRefreshTokenRepository refresh,
        IPasswordHasher hasher,
        ITokenService tokens)
    {
        _users = users;
        _refresh = refresh;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<Result<AuthTokensResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (!Guard.IsEmail(request.Email) || string.IsNullOrEmpty(request.Password))
            return Result<AuthTokensResponse>.Failure(ErrorCode.Unauthorized, "auth.invalid_credentials");

        var user = await _users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (user is null || !user.IsActive || !_hasher.Verify(request.Password, user.PasswordHash))
            return Result<AuthTokensResponse>.Failure(ErrorCode.Unauthorized, "auth.invalid_credentials");

        return await IssueAsync(user, ct);
    }

    public async Task<Result<AuthTokensResponse>> RefreshAsync(RefreshRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Result<AuthTokensResponse>.Failure(ErrorCode.Validation, "auth.refresh_required");

        var hash = _tokens.Hash(request.RefreshToken);
        var stored = await _refresh.GetByHashAsync(hash, ct);
        if (stored is null || !stored.IsActive)
            return Result<AuthTokensResponse>.Failure(ErrorCode.Unauthorized, "auth.refresh_invalid");

        var user = await _users.GetByIdAsync(stored.UserId, ct);
        if (user is null || !user.IsActive)
            return Result<AuthTokensResponse>.Failure(ErrorCode.Unauthorized, "auth.refresh_invalid");

        await _refresh.RevokeAsync(stored.Id, ct);
        return await IssueAsync(user, ct);
    }

    public async Task<Result<bool>> LogoutAsync(string userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<bool>.Failure(ErrorCode.Unauthorized, "auth.unauthorized");
        await _refresh.RevokeAllForUserAsync(userId, ct);
        return Result<bool>.Success(true);
    }

    public async Task<Result<UserResponse>> GetMeAsync(string userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<UserResponse>.Failure(ErrorCode.Unauthorized, "auth.unauthorized");

        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null || !user.IsActive)
            return Result<UserResponse>.Failure(ErrorCode.Unauthorized, "auth.unauthorized");

        return Result<UserResponse>.Success(MapUser(user));
    }

    public async Task<Result<UserResponse>> UpdateProfileAsync(
        string userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<UserResponse>.Failure(ErrorCode.Unauthorized, "auth.unauthorized");

        var fullName = request.FullName?.Trim() ?? string.Empty;
        if (fullName.Length < 2)
            return Result<UserResponse>.Failure(ErrorCode.Validation, "users.full_name_required");

        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null || !user.IsActive)
            return Result<UserResponse>.Failure(ErrorCode.Unauthorized, "auth.unauthorized");

        user.FullName = fullName;
        await _users.ReplaceAsync(user, ct);
        return Result<UserResponse>.Success(MapUser(user));
    }

    public async Task<Result<bool>> ChangePasswordAsync(
        string userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<bool>.Failure(ErrorCode.Unauthorized, "auth.unauthorized");

        if (string.IsNullOrEmpty(request.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword))
            return Result<bool>.Failure(ErrorCode.Validation, "auth.password_required");

        if (!Guard.IsStrongEnoughPassword(request.NewPassword))
            return Result<bool>.Failure(ErrorCode.Validation, "auth.password_too_short");

        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null || !user.IsActive)
            return Result<bool>.Failure(ErrorCode.Unauthorized, "auth.unauthorized");

        if (!_hasher.Verify(request.CurrentPassword, user.PasswordHash))
            return Result<bool>.Failure(ErrorCode.Validation, "auth.current_password_invalid");

        user.PasswordHash = _hasher.Hash(request.NewPassword);
        await _users.ReplaceAsync(user, ct);
        return Result<bool>.Success(true);
    }

    private static UserResponse MapUser(User user) =>
        new(user.Id, user.Email, user.FullName, user.Role, user.CreatedAt);

    private async Task<Result<AuthTokensResponse>> IssueAsync(User user, CancellationToken ct)
    {
        var access = _tokens.IssueAccessToken(user);
        var refresh = _tokens.IssueRefreshToken();
        await _refresh.InsertAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refresh.TokenHash,
            ExpiresAt = refresh.ExpiresAt,
        }, ct);

        var expiresIn = (long)(access.ExpiresAt - DateTime.UtcNow).TotalSeconds;
        return Result<AuthTokensResponse>.Success(new AuthTokensResponse(access.Token, refresh.Token, expiresIn));
    }
}
