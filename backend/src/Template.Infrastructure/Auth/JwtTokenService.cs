using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VoiceFlowStudio.Application.Common;
using VoiceFlowStudio.Core.Entities;

namespace VoiceFlowStudio.Infrastructure.Auth;

/// <summary>RS256-signed JWT issuer (Constitution IV).</summary>
public sealed class JwtTokenService : ITokenService, IDisposable
{
    private readonly JwtOptions _options;
    private readonly RSA _rsa;
    private readonly SigningCredentials _signing;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        if (string.IsNullOrWhiteSpace(_options.PrivateKeyPem))
            throw new InvalidOperationException("Jwt:PrivateKeyPem is not configured.");
        _rsa = RSA.Create();
        _rsa.ImportFromPem(_options.PrivateKeyPem);
        _signing = new SigningCredentials(new RsaSecurityKey(_rsa), SecurityAlgorithms.RsaSha256);
    }

    public IssuedAccessToken IssueAccessToken(User user)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_options.AccessTokenMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("role", user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: _signing);
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return new IssuedAccessToken(jwt, expires);
    }

    public IssuedRefreshToken IssueRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(bytes)
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return new IssuedRefreshToken(token, Hash(token), DateTime.UtcNow.AddDays(_options.RefreshTokenDays));
    }

    public string Hash(string refreshToken)
    {
        var bytes = Encoding.UTF8.GetBytes(refreshToken);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    public void Dispose() => _rsa.Dispose();
}
