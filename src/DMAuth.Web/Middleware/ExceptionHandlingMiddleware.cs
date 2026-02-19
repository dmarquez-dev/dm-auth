using DMAuth.Domain.Exceptions;
using FluentValidation;

namespace DMAuth.Web.Middleware;

/// <summary>
///		Middleware that catches unhandled exceptions and maps them to structured
///		JSON error responses with appropriate HTTP status codes.
/// </summary>
/// <param name="next">
///		The next middleware delegate in the pipeline.
/// </param>
/// <param name="logger">
///		Logger for recording exception details.
/// </param>
public sealed class ExceptionHandlingMiddleware(
	RequestDelegate next,
	ILogger<ExceptionHandlingMiddleware> logger)
{
	/// <summary>
	///		Invokes the middleware, catching and mapping any exceptions thrown
	///		by downstream pipeline components.
	/// </summary>
	/// <param name="context">
	///		The current HTTP context.
	/// </param>
	public async Task InvokeAsync(
		HttpContext context)
	{
		try
		{
			await next(
				context);
		}
		catch (ValidationException ex)
		{
			logger.LogInformation(
				"Validation failure on {Method} {Path}: {ErrorCount} error(s).",
				context.Request.Method,
				context.Request.Path,
				ex.Errors.Count());

			var errors = ex.Errors
				.GroupBy(failure =>
					failure.PropertyName)
				.ToDictionary(
					group =>
						group.Key,
					group =>
						group
							.Select(failure =>
								failure.ErrorMessage)
							.ToArray());

			context.Response.StatusCode = StatusCodes.Status400BadRequest;

			await context.Response.WriteAsJsonAsync(
				new
				{
					message = "One or more validation errors occurred.",
					errors
				});
		}
		catch (DomainException ex)
		{
			logger.LogWarning(
				"Domain rule violation on {Method} {Path}: {Message}",
				context.Request.Method,
				context.Request.Path,
				ex.Message);

			context.Response.StatusCode = StatusCodes.Status400BadRequest;

			await context.Response.WriteAsJsonAsync(
				new
				{
					message = ex.Message
				});
		}
		catch (Exception ex)
		{
			logger.LogError(
				ex,
				"Unhandled exception on {Method} {Path}.",
				context.Request.Method,
				context.Request.Path);

			context.Response.StatusCode = StatusCodes.Status500InternalServerError;

			await context.Response.WriteAsJsonAsync(
				new
				{
					message = "An unexpected error occurred."
				});
		}
	}
}
