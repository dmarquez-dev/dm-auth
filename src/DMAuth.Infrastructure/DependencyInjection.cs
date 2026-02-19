using DMAuth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
			options.UseSqlServer(
				configuration.GetConnectionString("DmAuthConnection")));

		return services;
	}
}
