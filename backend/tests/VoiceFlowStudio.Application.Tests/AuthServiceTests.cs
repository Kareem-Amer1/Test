using Moq;
using VoiceFlowStudio.Application.Common;
using VoiceFlowStudio.Application.Services;
using VoiceFlowStudio.Contracts.Auth;
using VoiceFlowStudio.Core.Entities;
using VoiceFlowStudio.Core.Interfaces;
using Xunit;

namespace VoiceFlowStudio.Application.Tests;

public class AuthServiceTests
{
    private static (AuthService svc, Mock<IUserRepository> users, Mock<IRefreshTokenRepository> refresh,
        Mock<IPasswordHasher> hasher, Mock<ITokenService> tokens) Build()
    {
        var users = new Mock<IUserRepository>();
        var refresh = new Mock<IRefreshTokenRepository>();
        var hasher = new Mock<IPasswordHasher>();
        var tokens = new Mock<ITokenService>();
        tokens.Setup(t => t.IssueAccessToken(It.IsAny<User>()))
            .Returns(new IssuedAccessToken("access", DateTime.UtcNow.AddMinutes(60)));
        tokens.Setup(t => t.IssueRefreshToken())
            .Returns(new IssuedRefreshToken("refresh", "hash", DateTime.UtcNow.AddDays(30)));
        tokens.Setup(t => t.Hash(It.IsAny<string>())).Returns<string>(s => $"H({s})");
        return (new AuthService(users.Object, refresh.Object, hasher.Object, tokens.Object),
            users, refresh, hasher, tokens);
    }

    [Fact]
    public async Task Register_NewEmail_Should_IssueTokens()
    {
        var (svc, users, refresh, hasher, _) = Build();
        users.Setup(u => u.GetByEmailAsync("a@b.com", default)).ReturnsAsync((User?)null);
        hasher.Setup(h => h.Hash("password123")).Returns("hashed");

        var result = await svc.RegisterAsync(new RegisterRequest("a@b.com", "password123"));

        Assert.True(result.IsSuccess);
        Assert.Equal("access", result.Value!.AccessToken);
        Assert.Equal("refresh", result.Value.RefreshToken);
        users.Verify(u => u.InsertAsync(It.Is<User>(x => x.Email == "a@b.com" && x.PasswordHash == "hashed"), default), Times.Once);
        refresh.Verify(r => r.InsertAsync(It.Is<RefreshToken>(rt => rt.TokenHash == "hash"), default), Times.Once);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Should_ReturnConflict()
    {
        var (svc, users, _, _, _) = Build();
        users.Setup(u => u.GetByEmailAsync("a@b.com", default))
            .ReturnsAsync(new User { Id = "1", Email = "a@b.com" });

        var result = await svc.RegisterAsync(new RegisterRequest("a@b.com", "password123"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.Conflict, result.Error.Code);
    }

    [Fact]
    public async Task Login_BadPassword_Should_ReturnUnauthorized()
    {
        var (svc, users, _, hasher, _) = Build();
        users.Setup(u => u.GetByEmailAsync("a@b.com", default))
            .ReturnsAsync(new User { Id = "1", Email = "a@b.com", PasswordHash = "h" });
        hasher.Setup(h => h.Verify("wrong", "h")).Returns(false);

        var result = await svc.LoginAsync(new LoginRequest("a@b.com", "wrong"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.Unauthorized, result.Error.Code);
    }

    [Fact]
    public async Task Login_ValidCredentials_Should_IssueTokens()
    {
        var (svc, users, _, hasher, _) = Build();
        users.Setup(u => u.GetByEmailAsync("a@b.com", default))
            .ReturnsAsync(new User { Id = "1", Email = "a@b.com", PasswordHash = "h" });
        hasher.Setup(h => h.Verify("ok-password", "h")).Returns(true);

        var result = await svc.LoginAsync(new LoginRequest("a@b.com", "ok-password"));

        Assert.True(result.IsSuccess);
        Assert.Equal("access", result.Value!.AccessToken);
    }

    [Fact]
    public async Task Refresh_InvalidToken_Should_ReturnUnauthorized()
    {
        var (svc, _, refresh, _, _) = Build();
        refresh.Setup(r => r.GetByHashAsync("H(rt)", default)).ReturnsAsync((RefreshToken?)null);

        var result = await svc.RefreshAsync(new RefreshRequest("rt"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.Unauthorized, result.Error.Code);
    }

    [Fact]
    public async Task Refresh_ValidToken_Should_RotateAndIssueTokens()
    {
        var (svc, users, refresh, _, _) = Build();
        var stored = new RefreshToken { Id = "rid", UserId = "1", TokenHash = "H(rt)", ExpiresAt = DateTime.UtcNow.AddDays(1) };
        refresh.Setup(r => r.GetByHashAsync("H(rt)", default)).ReturnsAsync(stored);
        users.Setup(u => u.GetByIdAsync("1", default)).ReturnsAsync(new User { Id = "1", Email = "a@b.com" });

        var result = await svc.RefreshAsync(new RefreshRequest("rt"));

        Assert.True(result.IsSuccess);
        refresh.Verify(r => r.RevokeAsync("rid", default), Times.Once);
        refresh.Verify(r => r.InsertAsync(It.IsAny<RefreshToken>(), default), Times.Once);
    }

    [Fact]
    public async Task Logout_Should_RevokeAllForUser()
    {
        var (svc, _, refresh, _, _) = Build();
        var result = await svc.LogoutAsync("1");
        Assert.True(result.IsSuccess);
        refresh.Verify(r => r.RevokeAllForUserAsync("1", default), Times.Once);
    }
}
