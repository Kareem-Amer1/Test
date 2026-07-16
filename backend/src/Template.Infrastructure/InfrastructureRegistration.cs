using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HireExam.Application.Common;
using HireExam.Application.Services;
using HireExam.Core.Interfaces;
using HireExam.Infrastructure.Auth;
using HireExam.Infrastructure.Mapping;
using HireExam.Infrastructure.Persistence;
using HireExam.Infrastructure.Repositories;
using HireExam.Infrastructure.Seed;

namespace HireExam.Infrastructure;

public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<MongoOptions>(config.GetSection(MongoOptions.Section));
        services.Configure<JwtOptions>(config.GetSection(JwtOptions.Section));
        services.Configure<SeedOptions>(config.GetSection(SeedOptions.Section));

        services.AddSingleton<IMongoClientFactory, MongoClientFactory>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISeedService, SeedService>();

        services.AddAutoMapper(typeof(UserProfile));
        return services;
    }
}
