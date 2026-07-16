using VoiceFlowStudio.Application.Common;
using VoiceFlowStudio.Application.Validation;
using VoiceFlowStudio.Contracts.Auth;
using VoiceFlowStudio.Core.Entities;
using VoiceFlowStudio.Core.Interfaces;

namespace VoiceFlowStudio.Application.Services;

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

    public async Task<Result<AuthTokensResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (!Guard.IsEmail(request.Email))
            return Result<AuthTokensResponse>.Failure(ErrorCode.Validation, "auth.email_invalid");
        if (!Guard.IsStrongEnoughPassword(request.Password))
            return Result<AuthTokensResponse>.Failure(ErrorCode.Validation, "auth.password_too_short");

        var existing = await _users.GetByEmailAsync(request.Email, ct);
        if (existing is not null)
            return Result<AuthTokensResponse>.Failure(ErrorCode.Conflict, "auth.email_taken");

        var user = new User
        {
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = _hasher.Hash(request.Password),
            Role = "user",
        };
        await _users.InsertAsync(user, ct);
        return await IssueAsync(user, ct);
    }

    public async Task<Result<AuthTokensResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (!Guard.IsEmail(request.Email) || string.IsNullOrEmpty(request.Password))
            return Result<AuthTokensResponse>.Failure(ErrorCode.Validation, "auth.invalid_credentials");

        var user = await _users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
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
        if (user is null)
            return Result<AuthTokensResponse>.Failure(ErrorCode.Unauthorized, "auth.refresh_invalid");

        // Single-document writes only (Constitution: no transactions). Revoke then issue.
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
