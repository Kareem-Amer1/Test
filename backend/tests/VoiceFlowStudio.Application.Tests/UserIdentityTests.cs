using HireExam.Application.Common;
using HireExam.Core.Entities;
using Xunit;

namespace HireExam.Application.Tests;

public class UserIdentityTests
{
    [Fact]
    public void CanAccessExam_SuperAdmin_AllowsAnyConductedBy()
    {
        Assert.True(UserIdentity.CanAccessExam("hr-1", UserRoles.SuperAdmin, "hr-2"));
    }

    [Fact]
    public void CanAccessExam_Hr_AllowsOwnExam()
    {
        const string id = "674abc123456789012345678";
        Assert.True(UserIdentity.CanAccessExam(id, UserRoles.HR, id));
    }

    [Fact]
    public void CanAccessExam_Hr_DeniesOtherExam()
    {
        Assert.False(UserIdentity.CanAccessExam("674abc123456789012345678", UserRoles.HR, "674def123456789012345678"));
    }

    [Fact]
    public void IsSameUser_IgnoresCase()
    {
        Assert.True(UserIdentity.IsSameUser("674ABC123456789012345678", "674abc123456789012345678"));
    }
}
