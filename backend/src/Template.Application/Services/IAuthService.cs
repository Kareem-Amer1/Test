using VoiceFlowStudio.Application.Common;
using VoiceFlowStudio.Contracts.Auth;

namespace VoiceFlowStudio.Application.Services;

/// <summary>
/// Constitution XIII: each service interface lives in its own file.
/// </summary>
public interface IAuthService
{
    Task<Result<AuthTokensResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<Result<AuthTokensResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<Result<AuthTokensResponse>> RefreshAsync(RefreshRequest request, CancellationToken ct = default);
    Task<Result<bool>> LogoutAsync(string userId, CancellationToken ct = default);
}
