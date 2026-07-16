using AutoMapper;
using VoiceFlowStudio.Contracts.Projects;
using VoiceFlowStudio.Core.Entities;

namespace VoiceFlowStudio.Infrastructure.Mapping;

/// <summary>Constitution VII: one AutoMapper profile per entity.</summary>
public sealed class ProjectProfile : Profile
{
    public ProjectProfile()
    {
        CreateMap<Project, ProjectResponse>();
        CreateMap<CreateProjectRequest, Project>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore());
    }
}
