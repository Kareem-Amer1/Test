using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VoiceFlowStudio.Application.Common;
using VoiceFlowStudio.Application.Services;
using VoiceFlowStudio.Core.Interfaces;
using VoiceFlowStudio.Infrastructure.Auth;
using VoiceFlowStudio.Infrastructure.Mapping;
using VoiceFlowStudio.Infrastructure.Persistence;
using VoiceFlowStudio.Infrastructure.Repositories;

namespace VoiceFlowStudio.Infrastructure;

public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<MongoOptions>(config.GetSection(MongoOptions.Section));
        services.Configure<JwtOptions>(config.GetSection(JwtOptions.Section));

        // Entity BSON mapping is declared via attributes on the entities; no
        // BsonClassMap registration is required here.

        services.AddSingleton<IMongoClientFactory, MongoClientFactory>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProjectService, ProjectService>();

        services.AddAutoMapper(typeof(ProjectProfile), typeof(UserProfile));
        return services;
    }
}
