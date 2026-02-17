using Microsoft.Extensions.DependencyInjection;

namespace DMAuth.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(
		this IServiceCollection services)
	{
		// DbContext, repositories, and external service registrations will be added here

		return services;
	}
}
