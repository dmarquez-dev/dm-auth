using System.Reflection;
using DMAuth.Application.Common.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DMAuth.Application;

/// <summary>
///		Registers application-layer services into the dependency injection container.
/// </summary>
public static class DependencyInjection
{
	/// <summary>
	///		Adds application services including MediatR, FluentValidation,
	///		and the validation pipeline behavior.
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
		{
			config.RegisterServicesFromAssembly(
				assembly);
			config.AddOpenBehavior(
				typeof(ValidationBehavior<,>));
		});

		services.AddValidatorsFromAssembly(
			assembly);

		return services;
	}
}
