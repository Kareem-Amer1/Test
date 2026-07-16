using HireExam.Application.Common;
using HireExam.Contracts.Auth;

namespace HireExam.Application.Services;

public interface IAuthService
{
    Task<Result<AuthTokensResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<Result<AuthTokensResponse>> RefreshAsync(RefreshRequest request, CancellationToken ct = default);
    Task<Result<bool>> LogoutAsync(string userId, CancellationToken ct = default);
    Task<Result<UserResponse>> GetMeAsync(string userId, CancellationToken ct = default);
}
