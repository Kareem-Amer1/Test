using VoiceFlowStudio.Core.Entities;

namespace VoiceFlowStudio.Application.Common;

public sealed record IssuedAccessToken(string Token, DateTime ExpiresAt);
public sealed record IssuedRefreshToken(string Token, string TokenHash, DateTime ExpiresAt);

public interface ITokenService
{
    IssuedAccessToken IssueAccessToken(User user);
    IssuedRefreshToken IssueRefreshToken();
    string Hash(string refreshToken);
}
