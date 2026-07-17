using Moq;
using HireExam.Application.Common;
using HireExam.Application.Services;
using HireExam.Contracts.Users;
using HireExam.Core.Entities;
using HireExam.Core.Interfaces;
using Xunit;

namespace HireExam.Application.Tests;

public class UserServiceTests
{
    private static (UserService svc, Mock<IUserRepository> users, Mock<IRefreshTokenRepository> refresh, Mock<IPasswordHasher> hasher) Build()
    {
        var users = new Mock<IUserRepository>();
        var refresh = new Mock<IRefreshTokenRepository>();
        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns<string>(p => $"hash:{p}");
        return (new UserService(users.Object, refresh.Object, hasher.Object), users, refresh, hasher);
    }

    [Fact]
    public async Task CreateHrUser_ValidRequest_InsertsUser()
    {
        var (svc, users, _, _) = Build();
        users.Setup(u => u.GetByEmailAsync("hr@hireexam.local", default)).ReturnsAsync((User?)null);

        var result = await svc.CreateHrUserAsync(
            "admin-id",
            new CreateHrUserRequest("hr@hireexam.local", "Password123", "HR User"));

        Assert.True(result.IsSuccess);
        users.Verify(u => u.InsertAsync(It.Is<User>(x =>
            x.Email == "hr@hireexam.local" &&
            x.Role == UserRoles.HR &&
            x.IsActive), default), Times.Once);
    }

    [Fact]
    public async Task CreateHrUser_DuplicateEmail_ReturnsConflict()
    {
        var (svc, users, _, _) = Build();
        users.Setup(u => u.GetByEmailAsync("hr@hireexam.local", default))
            .ReturnsAsync(new User { Email = "hr@hireexam.local" });

        var result = await svc.CreateHrUserAsync(
            "admin-id",
            new CreateHrUserRequest("hr@hireexam.local", "Password123", "HR User"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.Conflict, result.Error.Code);
    }

    [Fact]
    public async Task DeactivateHrUser_Self_ReturnsForbidden()
    {
        var (svc, _, _, _) = Build();

        var result = await svc.DeactivateHrUserAsync("same-id", "same-id");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.Forbidden, result.Error.Code);
        Assert.Equal("users.cannot_deactivate_self", result.Error.Message);
    }

    [Fact]
    public async Task DeactivateHrUser_SuperAdminTarget_ReturnsForbidden()
    {
        var (svc, users, _, _) = Build();
        users.Setup(u => u.GetByIdAsync("admin-id", default))
            .ReturnsAsync(new User { Id = "admin-id", Role = UserRoles.SuperAdmin, IsActive = true });

        var result = await svc.DeactivateHrUserAsync("admin-id", "actor-id");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.Forbidden, result.Error.Code);
        Assert.Equal("users.hr_only", result.Error.Message);
    }

    [Fact]
    public async Task DeactivateHrUser_ActiveHr_RevokesTokens()
    {
        var (svc, users, refresh, _) = Build();
        users.Setup(u => u.GetByIdAsync("hr-id", default))
            .ReturnsAsync(new User { Id = "hr-id", Role = UserRoles.HR, IsActive = true });
        users.Setup(u => u.ReplaceAsync(It.IsAny<User>(), default)).ReturnsAsync(true);

        var result = await svc.DeactivateHrUserAsync("hr-id", "admin-id");

        Assert.True(result.IsSuccess);
        refresh.Verify(r => r.RevokeAllForUserAsync("hr-id", default), Times.Once);
    }
}
