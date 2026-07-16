using AutoMapper;
using HireExam.Contracts.Auth;
using HireExam.Core.Entities;

namespace HireExam.Infrastructure.Mapping;

public sealed class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserResponse>()
            .ConstructUsing(u => new UserResponse(u.Id, u.Email, u.FullName, u.Role));
    }
}
