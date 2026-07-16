using AutoMapper;
using VoiceFlowStudio.Contracts.Auth;
using VoiceFlowStudio.Core.Entities;

namespace VoiceFlowStudio.Infrastructure.Mapping;

public sealed class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserResponse>();
    }
}
