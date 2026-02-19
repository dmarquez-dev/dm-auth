using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DMAuth.Application;

/// <summary>
///		Registers application-layer services into the dependency injection container.
/// </summary>
public static class DependencyInjection
{
	/// <summary>
	///		Adds applications services including MediatR.
	/// </summary>
	/// <param name="services">
	///		The service collection to register services into.
	/// </param>
	/// <returns>
	///		The service collection for chaining.
	/// </returns>
	public static IServiceCollection AddApplication(
		this IServiceCollection services)
	{
		var assembly = Assembly.GetExecutingAssembly();

		services.AddMediatR(config =>
			config.RegisterServicesFromAssembly(assembly));

		services.AddValidatorsFromAssembly(assembly);

		return services;
	}
}
