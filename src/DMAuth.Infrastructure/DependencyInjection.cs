using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Settings;
using DMAuth.Domain.Interfaces;
using DMAuth.Infrastructure.Persistence;
using DMAuth.Infrastructure.Persistence.Repositories;
using DMAuth.Infrastructure.Security;
using DMAuth.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DMAuth.Infrastructure;

/// <summary>
///		Registers infrastructure-layer services into the dependency injection container.
/// </summary>
public static class DependencyInjection
{
	/// <summary>
	///		Adds infrastructure services including the EF Core database context.
	/// </summary>
	/// <param name="services">
	///		The service collection to register services into.
	/// </param>
	/// <param name="configuration">
	///		The application configuration containing connection strings.
	/// </param>
	/// <returns>
	///		The service collection for chaining.
	/// </returns>
	public static IServiceCollection AddInfrastructure(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services.AddDbContext<DmAuthDbContext>(options =>
			options.UseSqlServer(configuration.GetConnectionString("DmAuth")));

		services.AddScoped<IUnitOfWork>(provider =>
			provider.GetRequiredService<DmAuthDbContext>());

		services.AddScoped<IUserRepository, UserRepository>();
		services.AddScoped<IClientRepository, ClientRepository>();
		services.AddScoped<IConsentRepository, ConsentRepository>();
		services.AddScoped<IAuthorizationCodeRepository, AuthorizationCodeRepository>();
		services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

		services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
		services.AddScoped<ITokenService, TokenService>();

		services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
		services.AddSingleton(provider =>
			provider.GetRequiredService<IOptions<JwtSettings>>().Value);

		services.Configure<OAuthSettings>(configuration.GetSection("OAuth"));
		services.AddSingleton(provider =>
			provider.GetRequiredService<IOptions<OAuthSettings>>().Value);

		return services;
	}
}
