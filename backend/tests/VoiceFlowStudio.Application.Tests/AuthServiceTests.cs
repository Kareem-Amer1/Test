using Moq;
using HireExam.Application.Common;
using HireExam.Application.Services;
using HireExam.Contracts.Auth;
using HireExam.Core.Entities;
using HireExam.Core.Interfaces;
using Xunit;

namespace HireExam.Application.Tests;

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
    public async Task Login_InactiveUser_Should_ReturnUnauthorized()
    {
        var (svc, users, _, hasher, _) = Build();
        users.Setup(u => u.GetByEmailAsync("a@b.com", default))
            .ReturnsAsync(new User { Id = "1", Email = "a@b.com", PasswordHash = "h", IsActive = false });
        hasher.Setup(h => h.Verify("ok-password", "h")).Returns(true);

        var result = await svc.LoginAsync(new LoginRequest("a@b.com", "ok-password"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.Unauthorized, result.Error.Code);
    }

    [Fact]
    public async Task Login_BadPassword_Should_ReturnUnauthorized()
    {
        var (svc, users, _, hasher, _) = Build();
        users.Setup(u => u.GetByEmailAsync("a@b.com", default))
            .ReturnsAsync(new User { Id = "1", Email = "a@b.com", PasswordHash = "h", IsActive = true });
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
            .ReturnsAsync(new User { Id = "1", Email = "a@b.com", PasswordHash = "h", IsActive = true, Role = UserRoles.HR });
        hasher.Setup(h => h.Verify("ok-password", "h")).Returns(true);

        var result = await svc.LoginAsync(new LoginRequest("a@b.com", "ok-password"));

        Assert.True(result.IsSuccess);
        Assert.Equal("access", result.Value!.AccessToken);
    }

    [Fact]
    public async Task GetMe_ValidUser_Should_ReturnProfile()
    {
        var (svc, users, _, _, _) = Build();
        users.Setup(u => u.GetByIdAsync("1", default))
            .ReturnsAsync(new User { Id = "1", Email = "a@b.com", FullName = "Admin", Role = UserRoles.SuperAdmin, IsActive = true });

        var result = await svc.GetMeAsync("1");

        Assert.True(result.IsSuccess);
        Assert.Equal("Admin", result.Value!.FullName);
        Assert.Equal(UserRoles.SuperAdmin, result.Value.Role);
    }

    [Fact]
    public async Task UpdateProfile_ValidName_Should_Persist()
    {
        var (svc, users, _, _, _) = Build();
        var user = new User
        {
            Id = "1",
            Email = "a@b.com",
            FullName = "Old Name",
            Role = UserRoles.HR,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
        users.Setup(u => u.GetByIdAsync("1", default)).ReturnsAsync(user);
        users.Setup(u => u.ReplaceAsync(user, default)).ReturnsAsync(true);

        var result = await svc.UpdateProfileAsync("1", new UpdateProfileRequest("New Name"));

        Assert.True(result.IsSuccess);
        Assert.Equal("New Name", result.Value!.FullName);
        Assert.Equal("New Name", user.FullName);
    }

    [Fact]
    public async Task ChangePassword_WrongCurrent_Should_Fail()
    {
        var (svc, users, _, hasher, _) = Build();
        users.Setup(u => u.GetByIdAsync("1", default))
            .ReturnsAsync(new User { Id = "1", PasswordHash = "h", IsActive = true });
        hasher.Setup(h => h.Verify("wrong", "h")).Returns(false);

        var result = await svc.ChangePasswordAsync("1", new ChangePasswordRequest("wrong", "new-password-1"));

        Assert.False(result.IsSuccess);
        Assert.Equal("auth.current_password_invalid", result.Error.Message);
    }

    [Fact]
    public async Task ChangePassword_Valid_Should_UpdateHash()
    {
        var (svc, users, _, hasher, _) = Build();
        var user = new User { Id = "1", PasswordHash = "h", IsActive = true };
        users.Setup(u => u.GetByIdAsync("1", default)).ReturnsAsync(user);
        users.Setup(u => u.ReplaceAsync(user, default)).ReturnsAsync(true);
        hasher.Setup(h => h.Verify("old-password", "h")).Returns(true);
        hasher.Setup(h => h.Hash("new-password-1")).Returns("new-hash");

        var result = await svc.ChangePasswordAsync("1", new ChangePasswordRequest("old-password", "new-password-1"));

        Assert.True(result.IsSuccess);
        Assert.Equal("new-hash", user.PasswordHash);
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
        users.Setup(u => u.GetByIdAsync("1", default))
            .ReturnsAsync(new User { Id = "1", Email = "a@b.com", IsActive = true });

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
