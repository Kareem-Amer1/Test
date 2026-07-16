using Moq;
using VoiceFlowStudio.Application.Common;
using VoiceFlowStudio.Application.Services;
using VoiceFlowStudio.Contracts.Projects;
using VoiceFlowStudio.Core.Entities;
using VoiceFlowStudio.Core.Interfaces;
using Xunit;

namespace VoiceFlowStudio.Application.Tests;

public class ProjectServiceTests
{
    private static (ProjectService svc, Mock<IProjectRepository> projects, Mock<IUserRepository> users) Build()
    {
        var projects = new Mock<IProjectRepository>();
        var users = new Mock<IUserRepository>();
        return (new ProjectService(projects.Object, users.Object), projects, users);
    }

    private static User UserWith(params string[] projectIds) =>
        new() { Id = "u1", Email = "a@b.com", ProjectIds = projectIds.ToList() };

    [Fact]
    public async Task List_Should_ReturnProjects()
    {
        var (svc, projects, users) = Build();
        users.Setup(u => u.GetByIdAsync("u1", default)).ReturnsAsync(UserWith("1"));
        projects.Setup(r => r.ListByIdsAsync(It.Is<IReadOnlyCollection<string>>(c => c.Contains("1")), null, default))
            .ReturnsAsync(new[] { new Project { Id = "1", Name = "A" } });

        var result = await svc.ListAsync("u1");

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    [Fact]
    public async Task List_NoProjects_Should_ReturnEmpty()
    {
        var (svc, _, users) = Build();
        users.Setup(u => u.GetByIdAsync("u1", default)).ReturnsAsync(UserWith());

        var result = await svc.ListAsync("u1");

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task Get_NotOwned_Should_ReturnNotFound()
    {
        var (svc, _, users) = Build();
        users.Setup(u => u.GetByIdAsync("u1", default)).ReturnsAsync(UserWith());

        var result = await svc.GetAsync("1", "u1");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Error.Code);
    }

    [Fact]
    public async Task Create_MissingName_Should_ReturnValidation()
    {
        var (svc, _, _) = Build();
        var result = await svc.CreateAsync("u1", new CreateProjectRequest(" ", "d", "#fff"));
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.Validation, result.Error.Code);
    }

    [Fact]
    public async Task Create_Should_InsertAndLinkToUser()
    {
        var (svc, projects, users) = Build();

        var result = await svc.CreateAsync("u1", new CreateProjectRequest("A", "d", "#fff"));

        Assert.True(result.IsSuccess);
        Assert.Equal("A", result.Value!.Name);
        projects.Verify(r => r.InsertAsync(It.Is<Project>(p => p.Name == "A"), default), Times.Once);
        users.Verify(u => u.AddProjectIdAsync("u1", It.IsAny<string>(), default), Times.Once);
    }

    [Fact]
    public async Task Update_NotOwned_Should_ReturnNotFound()
    {
        var (svc, _, users) = Build();
        users.Setup(u => u.GetByIdAsync("u1", default)).ReturnsAsync(UserWith());

        var result = await svc.UpdateAsync("1", "u1", new UpdateProjectRequest("X", null, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Error.Code);
    }

    [Fact]
    public async Task Update_Should_PatchAndPersist()
    {
        var (svc, projects, users) = Build();
        users.Setup(u => u.GetByIdAsync("u1", default)).ReturnsAsync(UserWith("1"));
        projects.Setup(r => r.GetByIdAsync("1", default)).ReturnsAsync(new Project { Id = "1", Name = "A" });
        projects.Setup(r => r.UpdateAsync(It.IsAny<Project>(), default)).ReturnsAsync(true);

        var result = await svc.UpdateAsync("1", "u1", new UpdateProjectRequest("B", null, null));

        Assert.True(result.IsSuccess);
        Assert.Equal("B", result.Value!.Name);
    }

    [Fact]
    public async Task Delete_NotOwned_Should_ReturnNotFound()
    {
        var (svc, _, users) = Build();
        users.Setup(u => u.GetByIdAsync("u1", default)).ReturnsAsync(UserWith());

        var result = await svc.DeleteAsync("1", "u1");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Error.Code);
    }

    [Fact]
    public async Task Delete_Should_Succeed_AndUnlinkFromUser()
    {
        var (svc, projects, users) = Build();
        users.Setup(u => u.GetByIdAsync("u1", default)).ReturnsAsync(UserWith("1"));
        projects.Setup(r => r.DeleteAsync("1", default)).ReturnsAsync(true);

        var result = await svc.DeleteAsync("1", "u1");

        Assert.True(result.IsSuccess);
        users.Verify(u => u.RemoveProjectIdAsync("u1", "1", default), Times.Once);
    }
}
