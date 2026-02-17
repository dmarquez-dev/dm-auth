using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DMAuth.Application;

public static class DependencyInjection
{
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
